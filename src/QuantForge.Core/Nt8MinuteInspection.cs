using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core;

public enum MarketPriceSeries { Last, Bid, Ask }
public enum MarketDataInspectionStatus { Inspected, Invalid, Unavailable, Cancelled }

/// <summary>User-declared labels: NT8 OHLCV rows do not prove instrument or price-series identity.</summary>
public sealed record Nt8MinuteDescriptor(string Instrument, MarketPriceSeries PriceSeries);

/// <summary>Structural inspection only. This is not research admission or benchmark evidence.</summary>
public sealed record Nt8MinuteInspectionResult(
    MarketDataInspectionStatus Status,
    string DiagnosticCode,
    Nt8MinuteDescriptor? DeclaredDescriptor = null,
    string? SourceFingerprint = null,
    IReadOnlyList<MarketEvent>? Bars = null,
    int NonContiguousIntervals = 0,
    int ErrorLine = 0);

public static class Nt8MinuteInspector
{
    public const int MaximumBytes = 32 * 1024 * 1024;
    public const int MaximumBars = 500_000;
    public const int MaximumLineCharacters = 256;

    // Explicitly scoped to UTC, end-stamped one-minute NT8 text exports.
    // Day/tick formats, timezone inference, deduplication and gap filling are not supported.
    public static async Task<Nt8MinuteInspectionResult> InspectAsync(
        Stream source, Nt8MinuteDescriptor descriptor, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(descriptor);
        if (string.IsNullOrWhiteSpace(descriptor.Instrument) || descriptor.Instrument.Length > 64 ||
            descriptor.Instrument != descriptor.Instrument.Trim() ||
            descriptor.Instrument.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is ' ' or '-' or '.')) ||
            !Enum.IsDefined(descriptor.PriceSeries))
            return Invalid("QF-DATA-DESCRIPTOR");
        try
        {
            // Bound encoded input and each line independently. Never retain a whole-file string.
            var buffer = new byte[64 * 1024];
            var line = new byte[MaximumLineCharacters * 4 + 4];
            var lineBytes = 0;
            var totalBytes = 0;
            var lineNumber = 0;
            var bars = new List<MarketEvent>();
            var discontinuities = 0;
            var utf8 = new UTF8Encoding(false, true);
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

            Nt8MinuteInspectionResult? ParseLine(bool eof)
            {
                lineNumber++;
                var offset = lineNumber == 1 && lineBytes >= 3 && line[0] == 0xEF && line[1] == 0xBB && line[2] == 0xBF ? 3 : 0;
                var text = utf8.GetString(line, offset, lineBytes - offset);
                if (text.EndsWith('\r')) text = text[..^1];
                if (eof && offset == 3 && lineBytes == 3) return null;
                if (text.Length == 0 || text.Length > MaximumLineCharacters)
                    return Invalid("QF-DATA-LINE", lineNumber);
                if (bars.Count == MaximumBars) return Invalid("QF-DATA-TOO-MANY-BARS", lineNumber);
                var fields = text.Split(';');
                if (fields.Length != 6 || fields[0].Length != 15 ||
                    !DateTimeOffset.TryParseExact(fields[0], "yyyyMMdd HHmmss", CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var stamp) || stamp.Second != 0)
                    return Invalid("QF-DATA-FORMAT", lineNumber);
                var values = new decimal[5];
                for (var i = 0; i < values.Length; i++)
                {
                    var value = fields[i + 1];
                    // Reject signs, exponent/group separators and excess precision rather than rounding.
                    if (value.Length == 0 || value.Length > 28 || value.Count(c => c == '.') > 1 ||
                        value.Any(c => !(char.IsAsciiDigit(c) || c == '.')) ||
                        !decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out values[i]))
                        return Invalid("QF-DATA-NUMBER", lineNumber);
                }
                if (decimal.Truncate(values[4]) != values[4]) return Invalid("QF-DATA-VOLUME", lineNumber);
                var bar = new MarketEvent(bars.Count, stamp, values[0], values[1], values[2], values[3], values[4]);
                try { bar.Validate(); }
                catch (InvalidOperationException) { return Invalid("QF-DATA-OHLCV", lineNumber); }
                if (bars.Count > 0)
                {
                    var delta = stamp - bars[^1].Timestamp;
                    if (delta <= TimeSpan.Zero) return Invalid("QF-DATA-ORDER", lineNumber);
                    if (delta != TimeSpan.FromMinutes(1)) discontinuities++;
                }
                bars.Add(bar);
                return null;
            }

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var request = Math.Min(buffer.Length, MaximumBytes - totalBytes + 1);
                var read = await source.ReadAsync(buffer.AsMemory(0, request), cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                if (read == 0) break;
                totalBytes += read;
                if (totalBytes > MaximumBytes) return Invalid("QF-DATA-TOO-LARGE");
                hash.AppendData(buffer, 0, read);
                for (var i = 0; i < read; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (buffer[i] == (byte)'\n')
                    {
                        var failure = ParseLine(false);
                        if (failure is not null) return failure;
                        lineBytes = 0;
                    }
                    else
                    {
                        if (lineBytes == line.Length) return Invalid("QF-DATA-LINE", lineNumber + 1);
                        line[lineBytes++] = buffer[i];
                    }
                }
            }
            if (lineBytes > 0)
            {
                var failure = ParseLine(true);
                if (failure is not null) return failure;
            }
            if (bars.Count == 0) return Invalid("QF-DATA-EMPTY");
            cancellationToken.ThrowIfCancellationRequested();
            return new(MarketDataInspectionStatus.Inspected, "QF-DATA-INSPECTED", descriptor,
                Convert.ToHexString(hash.GetHashAndReset()), bars.AsReadOnly(), discontinuities);
        }
        catch (OperationCanceledException) { return new(MarketDataInspectionStatus.Cancelled, "QF-DATA-CANCELLED"); }
        catch (DecoderFallbackException) { return Invalid("QF-DATA-ENCODING"); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or ObjectDisposedException or NotSupportedException)
        { return new(MarketDataInspectionStatus.Unavailable, "QF-DATA-UNAVAILABLE"); }
    }

    private static Nt8MinuteInspectionResult Invalid(string code, int line = 0) =>
        new(MarketDataInspectionStatus.Invalid, code, ErrorLine: line);
}
