namespace QuantForge.Core;

public sealed record MarketPairCheck(int Left, int Right, string Code, string Basis,
    int Matching = 0, int Conflicting = 0, int LeftOnly = 0, int RightOnly = 0);
public sealed record MarketCrossReport(IReadOnlyList<MarketPairCheck> Pairs, int OmittedPairs, string Limitation);

/// <summary>Observed agreement only; never completeness, independent provenance, reliability or admission.</summary>
public static class MarketCrossValidation
{
    public const int MaximumPairs = 128;
    public static MarketCrossReport Compare(MarketBatchResult batch, bool allowUtcCalendarDayComparison = false, CancellationToken token = default)
    {
        var pairs = new List<MarketPairCheck>();
        var omitted = 0;
        var files = batch.Completed ? batch.Files.Where(x => x.State == MarketFileState.Inspected && x.Label is not null &&
            x.Data is { Complete: true, Rows.Count: > 0 and <= MarketTextReader.MaximumRows }).ToArray() : Array.Empty<MarketBatchFile>();
        for (var i = 0; i < files.Length; i++)
        for (var j = i + 1; j < files.Length; j++)
        {
            token.ThrowIfCancellationRequested();
            var a = files[i]; var b = files[j];
            if (a.Label != b.Label) continue;
            if (pairs.Count == MaximumPairs) { omitted++; continue; }
            try { pairs.Add(ComparePair(a, b, allowUtcCalendarDayComparison, token)); }
            catch (OverflowException) { pairs.Add(new(a.Index, b.Index, "QF-CROSS-OVERFLOW", "No numeric result published")); }
            catch (ArgumentOutOfRangeException) { pairs.Add(new(a.Index, b.Index, "QF-CROSS-TIME-RANGE", "No numeric result published")); }
        }
        return new(pairs.AsReadOnly(), omitted,
            "Unverified labels; observed ranges only. No missing ticks/session completeness inferred. First/last aggregated buckets may be partial. Tick ties retain source order. Replay quotes are not treated as standalone Bid/Ask streams. No admission or reliability score.");
    }

    private static MarketPairCheck ComparePair(MarketBatchFile a, MarketBatchFile b, bool utcDays, CancellationToken token)
    {
        var ka = a.Data!.Kind; var kb = b.Data!.Kind;
        var left = a.Data.Rows!; var right = b.Data.Rows!;
        var basis = "Same-format observed rows";
        if (ka != kb)
        {
            if (ka == MarketTextKind.Day || kb == MarketTextKind.Day)
            {
                if (!utcDays) return new(a.Index, b.Index, "QF-CROSS-SESSION-DEFINITION-REQUIRED", "Daily cross-format comparison requires an explicit day-boundary policy");
                left = Aggregate(left, ka, true, token); right = Aggregate(right, kb, true, token);
                basis = "Exploratory UTC calendar days; NOT exchange-session validation";
            }
            else if (ka == MarketTextKind.Minute || kb == MarketTextKind.Minute)
            {
                left = Aggregate(left, ka, false, token); right = Aggregate(right, kb, false, token);
                basis = "Observed tick OHLCV vs UTC end-stamped minutes; buckets [start,end), boundary tick enters next minute";
            }
            else basis = "Tick/replay Last-price and volume sequence only; embedded quotes excluded";
        }
        var i = 0; var j = 0; var same = 0; var different = 0; var lo = 0; var ro = 0;
        while (i < left.Count || j < right.Count)
        {
            token.ThrowIfCancellationRequested();
            if (j == right.Count || (i < left.Count && left[i].Stamp < right[j].Stamp)) { lo++; i++; }
            else if (i == left.Count || right[j].Stamp < left[i].Stamp) { ro++; j++; }
            else
            {
                var x = left[i++]; var y = right[j++];
                var quotesMatch = ka != MarketTextKind.TickReplay || kb != MarketTextKind.TickReplay || (x.Bid == y.Bid && x.Ask == y.Ask);
                if (x.Open == y.Open && x.High == y.High && x.Low == y.Low && x.Close == y.Close && x.Volume == y.Volume && quotesMatch) same++;
                else different++;
            }
        }
        return new(a.Index, b.Index, same + different == 0 ? "QF-CROSS-NO-OVERLAP" : "QF-CROSS-COMPARED", basis, same, different, lo, ro);
    }

    private static IReadOnlyList<MarketTextRow> Aggregate(IReadOnlyList<MarketTextRow> rows, MarketTextKind kind, bool days, CancellationToken token)
    {
        if ((days && kind == MarketTextKind.Day) || (!days && kind == MarketTextKind.Minute)) return rows;
        var output = new List<MarketTextRow>();
        foreach (var row in rows)
        {
            token.ThrowIfCancellationRequested();
            var stamp = days ? new DateTimeOffset((kind == MarketTextKind.Minute ? row.Stamp.AddTicks(-1) : row.Stamp).UtcDateTime.Date, TimeSpan.Zero)
                : new DateTimeOffset(checked((row.Stamp.Ticks / TimeSpan.TicksPerMinute + 1) * TimeSpan.TicksPerMinute), TimeSpan.Zero);
            if (output.Count == 0 || output[^1].Stamp != stamp)
                output.Add(new(stamp, row.Open, row.High, row.Low, row.Close, row.Volume));
            else
            {
                var last = output[^1];
                output[^1] = last with { High = Math.Max(last.High, row.High), Low = Math.Min(last.Low, row.Low), Close = row.Close, Volume = checked(last.Volume + row.Volume) };
            }
        }
        return output.AsReadOnly();
    }
}
