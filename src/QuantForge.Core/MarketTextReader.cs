using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core;

public enum MarketTextKind { Unknown, Minute, Day, Tick, TickReplay }
public readonly record struct MarketTextRow(DateTimeOffset Stamp, decimal Open, decimal High, decimal Low,
    decimal Close, decimal Volume, decimal? Bid = null, decimal? Ask = null);
public sealed record MarketTextResult(string Code, MarketTextKind Kind = MarketTextKind.Unknown,
    IReadOnlyList<MarketTextRow>? Rows = null, string? Hash = null, long Bytes = 0, uint Crc32 = 0)
{
    public bool Complete => Code == "QF-BATCH-INSPECTED";
}

/// <summary>Structural text inspection. Rows/labels never confer provenance or research admission.</summary>
public static class MarketTextReader
{
    public const int MaximumBytes = 32 * 1024 * 1024;
    public const int MaximumRows = 500_000;
    private static readonly uint[] CrcTable = CreateCrcTable();

    public static async Task<MarketTextResult> ReadAsync(Stream source, int rowLimit = MaximumRows,
        Action<int>? countBytes = null, CancellationToken token = default)
    {
        var rows = new List<MarketTextRow>();
        var kind = MarketTextKind.Unknown;
        var buffer = new byte[65536];
        var line = new byte[1028];
        var used = 0;
        long total = 0;
        uint crc = uint.MaxValue;
        var lineNumber = 0;
        var utf8 = new UTF8Encoding(false, true);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        MarketTextResult Fail(string code) => new(code);
        string? Parse(bool eof)
        {
            lineNumber++;
            var offset = lineNumber == 1 && used >= 3 && line[0] == 239 && line[1] == 187 && line[2] == 191 ? 3 : 0;
            var text = utf8.GetString(line, offset, used - offset);
            if (text.EndsWith('\r')) text = text[..^1];
            if (eof && offset == 3 && used == 3) return null;
            if (text.Length is 0 or > 256) return "QF-BATCH-LINE";
            if (rows.Count >= Math.Min(rowLimit, MaximumRows)) return "QF-BATCH-ROW-LIMIT";
            var f = text.Split(';');
            var candidate = f.Length switch
            {
                6 when f[0].Length == 8 => MarketTextKind.Day,
                6 when f[0].Length == 15 => MarketTextKind.Minute,
                3 when f[0].Length is 15 or 23 => MarketTextKind.Tick,
                5 when f[0].Length is 15 or 23 => MarketTextKind.TickReplay,
                _ => MarketTextKind.Unknown
            };
            if (candidate == MarketTextKind.Unknown || (kind != MarketTextKind.Unknown && kind != candidate)) return "QF-BATCH-FORMAT";
            var format = f[0].Length == 8 ? "yyyyMMdd" : f[0].Length == 15 ? "yyyyMMdd HHmmss" : "yyyyMMdd HHmmss fffffff";
            if (!DateTimeOffset.TryParseExact(f[0], format, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var stamp) ||
                (candidate == MarketTextKind.Minute && stamp.Second != 0)) return "QF-BATCH-TIME";
            var v = new decimal[f.Length - 1];
            for (var i = 0; i < v.Length; i++)
            {
                var x = f[i + 1];
                if (x.Length is 0 or > 28 || x.Count(c => c == '.') > 1 ||
                    x.Any(c => !(char.IsAsciiDigit(c) || c == '.')) ||
                    !decimal.TryParse(x, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out v[i])) return "QF-BATCH-NUMBER";
            }
            var volume = v[^1];
            if (volume != decimal.Truncate(volume)) return "QF-BATCH-VOLUME";
            var row = candidate is MarketTextKind.Minute or MarketTextKind.Day
                ? new MarketTextRow(stamp, v[0], v[1], v[2], v[3], volume)
                : new MarketTextRow(stamp, v[0], v[0], v[0], v[0], volume,
                    candidate == MarketTextKind.TickReplay ? v[1] : null,
                    candidate == MarketTextKind.TickReplay ? v[2] : null);
            if (row.High < Math.Max(row.Open, row.Close) || row.Low > Math.Min(row.Open, row.Close)) return "QF-BATCH-OHLCV";
            if (rows.Count > 0 && (stamp < rows[^1].Stamp ||
                (stamp == rows[^1].Stamp && candidate is MarketTextKind.Minute or MarketTextKind.Day))) return "QF-BATCH-ORDER";
            kind = candidate;
            rows.Add(row);
            return null;
        }
        try
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                var n = await source.ReadAsync(buffer.AsMemory(0, (int)Math.Min(buffer.Length, MaximumBytes - total + 1)), token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                if (n == 0) break;
                total += n;
                countBytes?.Invoke(n);
                if (total > MaximumBytes) return Fail("QF-BATCH-BYTE-LIMIT");
                hash.AppendData(buffer, 0, n);
                for (var i = 0; i < n; i++)
                {
                    token.ThrowIfCancellationRequested();
                    crc = CrcTable[(crc ^ buffer[i]) & 255] ^ (crc >> 8);
                    if (buffer[i] == 10)
                    {
                        var error = Parse(false); if (error is not null) return Fail(error);
                        used = 0;
                    }
                    else { if (used == line.Length) return Fail("QF-BATCH-LINE"); line[used++] = buffer[i]; }
                }
            }
            if (used > 0) { var error = Parse(true); if (error is not null) return Fail(error); }
            token.ThrowIfCancellationRequested();
            return rows.Count == 0 ? Fail("QF-BATCH-EMPTY") : new("QF-BATCH-INSPECTED", kind,
                rows.AsReadOnly(), Convert.ToHexString(hash.GetHashAndReset()), total, ~crc);
        }
        catch (DecoderFallbackException) { return Fail("QF-BATCH-ENCODING"); }
        // Cancellation and provider/archive errors propagate to the transactional batch boundary.
    }

    private static uint[] CreateCrcTable()
    {
        var table = new uint[256];
        for (uint i = 0; i < table.Length; i++)
        {
            var c = i; for (var j = 0; j < 8; j++) c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
            table[i] = c;
        }
        return table;
    }
}
