using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core;

public sealed record WalkForwardSegmentPerformance(
    string SegmentId,
    string EvaluationJobId,
    decimal FinalEquity,
    decimal RealizedPnl,
    string SegmentFingerprint);

public sealed record WalkForwardPerformanceSummary(
    string WalkForwardResultFingerprint,
    int SegmentCount,
    decimal TotalOutOfSampleRealizedPnl,
    decimal MeanOutOfSampleFinalEquity,
    decimal MinimumOutOfSampleFinalEquity,
    decimal MaximumOutOfSampleFinalEquity,
    IReadOnlyList<WalkForwardSegmentPerformance> Segments,
    string SummaryFingerprint);

public static class WalkForwardPerformanceSummaryRules
{
    public static WalkForwardPerformanceSummary Create(WalkForwardResearchResult result)
    {
        WalkForwardResearchResultRules.Validate(result);
        if (!result.Complete)
            throw new InvalidOperationException("Out-of-sample performance summary requires a complete walk-forward result.");

        var segments = result.Segments.Select(segment =>
        {
            if (segment.Evaluation is not { } evaluation ||
                evaluation.Status != ResearchResultStatus.Complete ||
                evaluation.Account is not { } account)
                throw new InvalidOperationException("Out-of-sample performance summary requires complete simulated evaluation state for every segment.");
            return new WalkForwardSegmentPerformance(
                segment.SegmentId,
                evaluation.JobId,
                account.Equity,
                account.RealizedPnl,
                segment.SegmentFingerprint);
        }).ToArray();

        var totalRealized = segments.Sum(x => x.RealizedPnl);
        var meanEquity = segments.Average(x => x.FinalEquity);
        var minEquity = segments.Min(x => x.FinalEquity);
        var maxEquity = segments.Max(x => x.FinalEquity);
        var payload = new StringBuilder()
            .Append(result.ResultFingerprint).Append('\n')
            .Append(segments.Length).Append('|')
            .Append(totalRealized.ToString("G29", System.Globalization.CultureInfo.InvariantCulture)).Append('|')
            .Append(meanEquity.ToString("G29", System.Globalization.CultureInfo.InvariantCulture)).Append('|')
            .Append(minEquity.ToString("G29", System.Globalization.CultureInfo.InvariantCulture)).Append('|')
            .Append(maxEquity.ToString("G29", System.Globalization.CultureInfo.InvariantCulture)).Append('\n');
        foreach (var segment in segments)
            payload.Append(segment.SegmentId).Append('|')
                .Append(segment.EvaluationJobId).Append('|')
                .Append(segment.FinalEquity.ToString("G29", System.Globalization.CultureInfo.InvariantCulture)).Append('|')
                .Append(segment.RealizedPnl.ToString("G29", System.Globalization.CultureInfo.InvariantCulture)).Append('|')
                .Append(segment.SegmentFingerprint).Append('\n');
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload.ToString())));

        return new WalkForwardPerformanceSummary(
            result.ResultFingerprint,
            segments.Length,
            totalRealized,
            meanEquity,
            minEquity,
            maxEquity,
            segments,
            fingerprint);
    }
}
