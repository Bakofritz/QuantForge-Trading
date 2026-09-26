namespace QuantForge.Core;

public enum MinuteComparisonStatus { Compared, Invalid, Cancelled }

/// <summary>Agreement between two declared sources; never live-benchmark or admission evidence.</summary>
public sealed record MinuteSeriesComparisonResult(
    MinuteComparisonStatus Status,
    string DiagnosticCode,
    string? PrimaryFingerprint = null,
    string? ReferenceFingerprint = null,
    int MatchingBars = 0,
    int ConflictingBars = 0,
    int PrimaryOnlyBars = 0,
    int ReferenceOnlyBars = 0)
{
    public bool SameSourceBytes => Status == MinuteComparisonStatus.Compared &&
        string.Equals(PrimaryFingerprint, ReferenceFingerprint, StringComparison.OrdinalIgnoreCase);
}

public static class MinuteSeriesComparison
{
    // Counts the entire union of observed timestamps, never an inferred calendar or a clipped overlap.
    public static MinuteSeriesComparisonResult Compare(
        Nt8MinuteInspectionResult primary, Nt8MinuteInspectionResult reference,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(primary);
        ArgumentNullException.ThrowIfNull(reference);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!ValidInspection(primary, cancellationToken) || !ValidInspection(reference, cancellationToken))
                return new(MinuteComparisonStatus.Invalid, "QF-COMPARE-INSPECTION");
            if (primary.DeclaredDescriptor != reference.DeclaredDescriptor)
                return new(MinuteComparisonStatus.Invalid, "QF-COMPARE-IDENTITY");
            var left = primary.Bars!;
            var right = reference.Bars!;
            var i = 0;
            var j = 0;
            var matching = 0;
            var conflicts = 0;
            var primaryOnly = 0;
            var referenceOnly = 0;
            while (i < left.Count || j < right.Count)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (j == right.Count || (i < left.Count && left[i].Timestamp < right[j].Timestamp))
                { primaryOnly++; i++; }
                else if (i == left.Count || right[j].Timestamp < left[i].Timestamp)
                { referenceOnly++; j++; }
                else
                {
                    var a = left[i++];
                    var b = right[j++];
                    if (a.Open == b.Open && a.High == b.High && a.Low == b.Low && a.Close == b.Close && a.Volume == b.Volume)
                        matching++;
                    else conflicts++;
                }
            }
            cancellationToken.ThrowIfCancellationRequested();
            return new(MinuteComparisonStatus.Compared, "QF-COMPARE-COMPARED", primary.SourceFingerprint,
                reference.SourceFingerprint, matching, conflicts, primaryOnly, referenceOnly);
        }
        catch (OperationCanceledException)
        { return new(MinuteComparisonStatus.Cancelled, "QF-COMPARE-CANCELLED"); }
    }

    private static bool ValidInspection(Nt8MinuteInspectionResult inspection, CancellationToken token)
    {
        if (inspection.Status != MarketDataInspectionStatus.Inspected ||
            inspection.DeclaredDescriptor is not { } descriptor ||
            string.IsNullOrWhiteSpace(descriptor.Instrument) || !Enum.IsDefined(descriptor.PriceSeries) ||
            inspection.SourceFingerprint is not { Length: 64 } fingerprint ||
            fingerprint.Any(c => !(char.IsAsciiDigit(c) || c is >= 'A' and <= 'F' or >= 'a' and <= 'f')) ||
            inspection.Bars is not { Count: > 0 and <= Nt8MinuteInspector.MaximumBars } bars)
            return false;
        for (var i = 0; i < bars.Count; i++)
        {
            token.ThrowIfCancellationRequested();
            var bar = bars[i];
            if (bar.Timestamp.Offset != TimeSpan.Zero || bar.Timestamp.Ticks % TimeSpan.TicksPerMinute != 0 ||
                bar.Sequence != i || bar.Volume != decimal.Truncate(bar.Volume) ||
                (i > 0 && bar.Timestamp <= bars[i - 1].Timestamp)) return false;
            try { bar.Validate(); }
            catch (InvalidOperationException) { return false; }
        }
        return true;
    }
}
