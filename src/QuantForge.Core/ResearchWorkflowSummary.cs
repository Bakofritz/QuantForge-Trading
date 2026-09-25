using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core;

public sealed record ResearchWorkflowSummary(
    string WorkflowFingerprint,
    ResearchBatchMode Mode,
    int TotalRuns,
    int CompleteRuns,
    int DataBlockedRuns,
    int InvalidRuns,
    decimal? MinimumReliabilityScore,
    decimal? AverageReliabilityScore,
    IReadOnlyList<ResearchReport> Reports,
    IReadOnlyList<DataReliabilityAssessment> Reliability);

public static class ResearchWorkflowSummaryFactory
{
    public static ResearchWorkflowSummary Create(
        ResearchBatchMode mode,
        ResearchWorkflowResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.Reports is null || result.Reports.Count == 0)
            throw new InvalidOperationException("A workflow summary requires at least one research report.");

        if (result.ComponentStatuses is null || result.ComponentStatuses.Count != result.Reports.Count)
            throw new InvalidOperationException("Workflow summary requires one terminal component status per report.");

        if (result.Reliability is null)
            throw new InvalidOperationException("Workflow summary requires the workflow reliability collection.");

        foreach (var report in result.Reports)
            ResearchReportRules.Validate(report);

        foreach (var status in result.ComponentStatuses)
            ResearchComponentStatusRules.RequireTerminal(status);

        foreach (var reliability in result.Reliability)
            DataReliabilityRules.Validate(reliability);

        var complete = result.Reports.Count(x => x.Status == ResearchResultStatus.Complete);
        var blocked = result.Reports.Count(x => x.Status == ResearchResultStatus.DataBlocked);
        var invalid = result.Reports.Count(x => x.Status == ResearchResultStatus.Invalid);
        decimal? minimumReliability = result.Reliability.Count == 0
            ? null
            : result.Reliability.Min(x => x.ScorePercent);
        decimal? averageReliability = result.Reliability.Count == 0
            ? null
            : result.Reliability.Average(x => x.ScorePercent);

        var fingerprint = Fingerprint(mode, result.Reports, result.Reliability);

        return new ResearchWorkflowSummary(
            fingerprint,
            mode,
            result.Reports.Count,
            complete,
            blocked,
            invalid,
            minimumReliability,
            averageReliability,
            result.Reports.ToArray(),
            result.Reliability.ToArray());
    }

    private static string Fingerprint(
        ResearchBatchMode mode,
        IReadOnlyList<ResearchReport> reports,
        IReadOnlyList<DataReliabilityAssessment> reliability)
    {
        var lines = new List<string> { $"mode={mode}" };

        foreach (var report in reports.OrderBy(x => x.JobId, StringComparer.Ordinal))
        {
            var account = report.Account is { } snapshot
                ? string.Join(
                    ",",
                    snapshot.LedgerNamespace,
                    Decimal(snapshot.StartingCash),
                    Decimal(snapshot.Cash),
                    Decimal(snapshot.Position.Quantity),
                    Decimal(snapshot.Position.AveragePrice),
                    Decimal(snapshot.MarketPrice),
                    Decimal(snapshot.RealizedPnl),
                    Decimal(snapshot.UnrealizedPnl),
                    Decimal(snapshot.Equity))
                : string.Empty;

            lines.Add(string.Join(
                "|",
                "report",
                report.JobId,
                report.Status,
                report.DatasetFingerprint,
                report.StrategyFingerprint,
                report.ExecutionPolicyFingerprint,
                report.ParameterFingerprint,
                report.TemporalPartition,
                report.BlockReason ?? string.Empty,
                account,
                report.EvidenceTail?.Fingerprint ?? string.Empty));
        }

        foreach (var assessment in reliability.OrderBy(x => x.DatasetFingerprint, StringComparer.Ordinal))
        {
            lines.Add(string.Join(
                "|",
                "reliability",
                assessment.DatasetFingerprint,
                Decimal(assessment.ScorePercent),
                assessment.ComparedToLiveBenchmark,
                assessment.UnresolvedGapCount,
                assessment.ConflictingOverlapCount,
                assessment.Limitation ?? string.Empty));
        }

        var payload = string.Join("\n", lines);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
    }

    private static string Decimal(decimal value) =>
        value.ToString(CultureInfo.InvariantCulture);
}

public static class ResearchWorkflowMarkdownExporter
{
    public static string Export(ResearchWorkflowSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        var builder = new StringBuilder();
        builder.AppendLine("# QuantForge Research Workflow Report");
        builder.AppendLine();
        builder.AppendLine($"- Workflow fingerprint: `{summary.WorkflowFingerprint}`");
        builder.AppendLine($"- Mode: {summary.Mode}");
        builder.AppendLine($"- Total runs: {summary.TotalRuns}");
        builder.AppendLine($"- Complete: {summary.CompleteRuns}");
        builder.AppendLine($"- Data blocked: {summary.DataBlockedRuns}");
        builder.AppendLine($"- Invalid: {summary.InvalidRuns}");
        builder.AppendLine($"- Minimum data reliability: {Score(summary.MinimumReliabilityScore)}");
        builder.AppendLine($"- Average data reliability: {Score(summary.AverageReliabilityScore)}");
        builder.AppendLine();
        builder.AppendLine("## Data reliability");
        builder.AppendLine();
        builder.AppendLine("| Dataset fingerprint | Score | Live benchmark | Unresolved gaps | Conflicting overlaps | Limitation |");
        builder.AppendLine("| --- | ---: | --- | ---: | ---: | --- |");
        foreach (var reliability in summary.Reliability.OrderBy(x => x.DatasetFingerprint, StringComparer.Ordinal))
        {
            builder.AppendLine($"| {Escape(reliability.DatasetFingerprint)} | {Score(reliability.ScorePercent)} | {reliability.ComparedToLiveBenchmark} | {reliability.UnresolvedGapCount} | {reliability.ConflictingOverlapCount} | {Escape(reliability.Limitation)} |");
        }

        builder.AppendLine();
        builder.AppendLine("## Research runs");
        builder.AppendLine();
        builder.AppendLine("| Job | Status | Dataset | Strategy | Parameters | Partition | Evidence | Reason |");
        builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- |");
        foreach (var report in summary.Reports.OrderBy(x => x.JobId, StringComparer.Ordinal))
        {
            builder.AppendLine($"| {Escape(report.JobId)} | {report.Status} | {Escape(report.DatasetFingerprint)} | {Escape(report.StrategyFingerprint)} | {Escape(report.ParameterFingerprint)} | {Escape(report.TemporalPartition)} | {Escape(report.EvidenceTail?.Fingerprint)} | {Escape(report.BlockReason)} |");
        }

        return builder.ToString();
    }

    private static string Score(decimal? value) =>
        value is null
            ? "n/a"
            : value.Value.ToString("0.##", CultureInfo.InvariantCulture) + "%";

    private static string Escape(string? value) =>
        string.IsNullOrEmpty(value)
            ? string.Empty
            : value.Replace("|", "\\|", StringComparison.Ordinal)
                .Replace("\r", " ", StringComparison.Ordinal)
                .Replace("\n", " ", StringComparison.Ordinal);
}
