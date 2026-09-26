using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace QuantForge.Core;

public sealed record MarketBatchSource(string Name, Func<Task<Stream>> Open);
public enum MarketFileState { Inspected, UnresolvedIdentity, Rejected, Duplicate, IdentityConflict }
public sealed record MarketBatchFile(int Index, string Name, string? ArchiveHash, MarketFileState State,
    string Code, Nt8MinuteDescriptor? Label, string LabelBasis, MarketTextResult? Data, int? DuplicateOf = null);
public sealed record MarketBatchResult(string Code, IReadOnlyList<MarketBatchFile> Files)
{
    public bool Completed => Code == "QF-BATCH-COMPLETED";
}

public static class MarketBatchInspection
{
    public const int MaximumFiles = 64;
    public const int MaximumZipBytes = 64 * 1024 * 1024;
    public const long MaximumExpandedBytes = 128 * 1024 * 1024;
    public const int MaximumRetainedRows = 1_000_000;
    private static readonly Regex ContractToken = new(@"(?<![A-Z0-9])(?<root>[A-Z][A-Z0-9]{0,11})[ _]+(?<expiry>(?:0[1-9]|1[0-2])-[0-9]{2})(?![A-Z0-9])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex SeriesToken = new(@"(?<![A-Z0-9])(?:Last|Bid|Ask)(?![A-Z0-9])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    // Flexible labels tolerate minute/day/tick annotations. They are not authenticated identity.
    public static Nt8MinuteDescriptor? DetectLabel(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 240 || name.Any(char.IsControl) || name.Contains('/') || name.Contains('\\')) return null;
        var c = ContractToken.Matches(name); var s = SeriesToken.Matches(name);
        if (c.Count != 1 || s.Count != 1) return null;
        return new(c[0].Groups["root"].Value.ToUpperInvariant() + " " + c[0].Groups["expiry"].Value,
            Enum.Parse<MarketPriceSeries>(s[0].Value, true));
    }

    public static async Task<MarketBatchResult> InspectAsync(IEnumerable<MarketBatchSource> sources, CancellationToken token = default)
    {
        var results = new List<MarketBatchFile>();
        long expanded = 0, archiveBytes = 0;
        var rows = 0;
        void Count(int n) { expanded += n; if (expanded > MaximumExpandedBytes) throw new BatchLimitException(); }
        void Add(string name, string? archiveHash, MarketTextResult? data, string? rejection = null)
        {
            if (results.Count == MaximumFiles) throw new BatchLimitException();
            var label = DetectLabel(name.Split('/')[^1]);
            if (data?.Complete == true && data.Kind == MarketTextKind.TickReplay && label is { PriceSeries: not MarketPriceSeries.Last }) rejection = "QF-BATCH-REPLAY-SERIES-CONFLICT";
            var status = rejection is not null || data?.Complete != true ? MarketFileState.Rejected : label is null ? MarketFileState.UnresolvedIdentity : MarketFileState.Inspected;
            results.Add(new(results.Count + 1, name, archiveHash, status, rejection ?? data?.Code ?? "QF-BATCH-UNSUPPORTED", label,
                label is null ? "Unresolved: no reliable instrument evidence" : "Unverified filename label", rejection is null ? data : null));
            if (data?.Complete == true && rejection is null) rows += data.Rows!.Count;
        }
        try
        {
            var selected = sources.Take(MaximumFiles + 1).ToArray();
            if (selected.Length is 0 or > MaximumFiles) return new("QF-BATCH-SELECTION-LIMIT", Array.Empty<MarketBatchFile>());
            foreach (var source in selected)
            {
                token.ThrowIfCancellationRequested();
                if (source.Name.Length > 240 || source.Name.Any(char.IsControl) || source.Name.Contains('/') || source.Name.Contains('\\'))
                { Add("Invalid source name", null, null, "QF-BATCH-NAME"); continue; }
                if (source.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    using var stream = await source.Open().ConfigureAwait(false);
                    using var staged = new MemoryStream();
                    var buffer = new byte[65536];
                    while (true)
                    {
                        var n = await stream.ReadAsync(buffer, token).ConfigureAwait(false); if (n == 0) break;
                        token.ThrowIfCancellationRequested(); archiveBytes += n;
                        if (archiveBytes > MaximumZipBytes) throw new BatchLimitException();
                        await staged.WriteAsync(buffer.AsMemory(0, n), token).ConfigureAwait(false);
                    }
                    var archiveHash = Convert.ToHexString(SHA256.HashData(staged.GetBuffer().AsSpan(0, (int)staged.Length)));
                    staged.Position = 0;
                    using var archive = new ZipArchive(staged, ZipArchiveMode.Read, true);
                    if (archive.Entries.Count > MaximumFiles) throw new BatchLimitException();
                    if (!archive.Entries.Any(e => !e.FullName.EndsWith('/'))) Add(source.Name, archiveHash, null, "QF-BATCH-EMPTY-ARCHIVE");
                    var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var entry in archive.Entries)
                    {
                        var path = entry.FullName;
                        var parts = path.TrimEnd('/').Split('/');
                        if (path.Length is 0 or > 240 || path.Any(char.IsControl) || path.Contains('\\') || path.Contains(':') ||
                            parts.Any(x => x is "" or "." or "..") || !paths.Add(path))
                            return new("QF-BATCH-UNSAFE-ARCHIVE", Array.Empty<MarketBatchFile>());
                        // No filesystem extraction. Reject symbolic links rather than interpreting targets.
                        if (((entry.ExternalAttributes >> 16) & 0xF000) == 0xA000) return new("QF-BATCH-UNSAFE-ARCHIVE", Array.Empty<MarketBatchFile>());
                    }
                    foreach (var entry in archive.Entries)
                    {
                        token.ThrowIfCancellationRequested();
                        if (entry.FullName.EndsWith('/')) continue;
                        if (results.Count == MaximumFiles) throw new BatchLimitException();
                        if (!entry.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                        { Add(entry.FullName, archiveHash, null, "QF-BATCH-UNSUPPORTED-MEMBER"); continue; }
                        if (entry.Length > MarketTextReader.MaximumBytes)
                        { Add(entry.FullName, archiveHash, null, "QF-BATCH-BYTE-LIMIT"); continue; }
                        using var member = entry.Open();
                        var data = await MarketTextReader.ReadAsync(member, MaximumRetainedRows - rows, Count, token).ConfigureAwait(false);
                        if (data.Complete && (data.Bytes != entry.Length || data.Crc32 != entry.Crc32))
                            return new("QF-BATCH-ARCHIVE-INTEGRITY", Array.Empty<MarketBatchFile>());
                        Add(entry.FullName, archiveHash, data);
                    }
                }
                else if (source.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    if (results.Count == MaximumFiles) throw new BatchLimitException();
                    using var stream = await source.Open().ConfigureAwait(false);
                    Add(source.Name, null, await MarketTextReader.ReadAsync(stream, MaximumRetainedRows - rows, Count, token).ConfigureAwait(false));
                }
                else Add(source.Name, null, null, "QF-BATCH-UNSUPPORTED");
            }
            // Matching bytes can associate an unlabeled copy with a labelled source, never authenticate either.
            foreach (var group in results.Where(x => x.Data?.Complete == true).GroupBy(x => x.Data!.Hash).ToArray())
            {
                token.ThrowIfCancellationRequested();
                var labels = group.Where(x => x.Label is not null).Select(x => x.Label!).Distinct().ToArray();
                if (labels.Length > 1)
                {
                    foreach (var x in group) results[x.Index - 1] = x with { State = MarketFileState.IdentityConflict, Code = "QF-BATCH-IDENTITY-CONFLICT", LabelBasis = "Same bytes carry conflicting labels; review required" };
                    continue;
                }
                var first = group.First();
                foreach (var x in group)
                {
                    var inferred = x.Label is null && labels.Length == 1;
                    var item = inferred ? x with { Label = labels[0], State = MarketFileState.Inspected, LabelBasis = "Associated by exact bytes with an unverified labelled source" } : x;
                    if (x.Index != first.Index) item = item with { State = MarketFileState.Duplicate, DuplicateOf = first.Index, Code = "QF-BATCH-DUPLICATE" };
                    results[x.Index - 1] = item;
                }
            }
            token.ThrowIfCancellationRequested();
            return new("QF-BATCH-COMPLETED", results.AsReadOnly());
        }
        catch (OperationCanceledException) { return new("QF-BATCH-CANCELLED", Array.Empty<MarketBatchFile>()); }
        catch (BatchLimitException) { return new("QF-BATCH-TOTAL-LIMIT", Array.Empty<MarketBatchFile>()); }
        catch (Exception error) when (error is IOException or InvalidDataException or UnauthorizedAccessException or NotSupportedException or ArgumentException or ObjectDisposedException)
        { return new("QF-BATCH-PROVIDER-OR-ARCHIVE-ERROR", Array.Empty<MarketBatchFile>()); }
    }
    private sealed class BatchLimitException : Exception { }
}
