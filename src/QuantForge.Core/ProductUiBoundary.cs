namespace QuantForge.Core;

public enum ProductUiOperation
{
    ImportMarketData,
    AuditStrategy,
    StartResearch,
    StartOptimization,
    ViewResearchSummary,
    ExportResearchReport
}

public readonly record struct ProductUiCommand(
    ProductUiOperation Operation,
    AuthorityDomain Authority);

public sealed record ProductUiRunState(
    string JobId,
    ResearchResultStatus Status,
    string DatasetFingerprint,
    string StrategyFingerprint,
    string? Message,
    string? EvidenceFingerprint);

public sealed record ProductUiWorkflowState(
    string WorkflowFingerprint,
    ResearchBatchMode Mode,
    int TotalRuns,
    int CompleteRuns,
    int DataBlockedRuns,
    int InvalidRuns,
    decimal? MinimumReliabilityScore,
    decimal? AverageReliabilityScore,
    bool LiveAccountEnabled,
    bool CanSubmitOrders,
    bool CanChangeApplicationSettings,
    IReadOnlyList<ProductUiRunState> Runs,
    IReadOnlyList<DataReliabilityAssessment> Reliability);

public static class ProductUiBoundary
{
    public static ResearchAuthorityDecision RequireResearchOnly(ProductUiCommand command)
    {
        var decision = AuthorityBoundary.EvaluateResearch(command.Authority);

        if (decision.CanSubmitOrders || decision.CanChangeApplicationSettings)
            throw new InvalidOperationException("Product UI research commands cannot acquire trading or settings authority.");

        return decision;
    }

    public static ProductUiWorkflowState CreateReadOnlyState(ResearchWorkflowSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        if (!Enum.IsDefined(summary.Mode))
            throw new InvalidOperationException("Unknown research mode cannot be presented.");

        if (string.IsNullOrWhiteSpace(summary.WorkflowFingerprint))
            throw new InvalidOperationException("Product UI workflow state requires a workflow fingerprint.");

        if (summary.Reports is null || summary.Reports.Count != summary.TotalRuns)
            throw new InvalidOperationException("Product UI workflow state requires the complete report collection.");

        if (summary.Reliability is null)
            throw new InvalidOperationException("Product UI workflow state requires data reliability information.");

        if (summary.TotalRuns < 1 ||
            summary.CompleteRuns < 0 ||
            summary.DataBlockedRuns < 0 ||
            summary.InvalidRuns < 0 ||
            summary.CompleteRuns + summary.DataBlockedRuns + summary.InvalidRuns != summary.TotalRuns)
            throw new InvalidOperationException("Product UI workflow counts are inconsistent.");

        foreach (var report in summary.Reports)
            ResearchReportRules.Validate(report);

        if (summary.Reports.Select(x => x.JobId).Distinct(StringComparer.Ordinal).Count() != summary.TotalRuns)
            throw new InvalidOperationException("Product UI requires unique research job identities.");

        foreach (var reliability in summary.Reliability)
            DataReliabilityRules.Validate(reliability);

        var runs = summary.Reports
            .OrderBy(x => x.JobId, StringComparer.Ordinal)
            .Select(report => new ProductUiRunState(
                report.JobId,
                report.Status,
                report.DatasetFingerprint,
                report.StrategyFingerprint,
                report.BlockReason,
                report.EvidenceTail?.Fingerprint))
            .ToArray();

        return new ProductUiWorkflowState(
            summary.WorkflowFingerprint,
            summary.Mode,
            summary.TotalRuns,
            summary.CompleteRuns,
            summary.DataBlockedRuns,
            summary.InvalidRuns,
            summary.MinimumReliabilityScore,
            summary.AverageReliabilityScore,
            LiveAccountEnabled: false,
            CanSubmitOrders: false,
            CanChangeApplicationSettings: false,
            runs,
            summary.Reliability.ToArray());
    }
}
