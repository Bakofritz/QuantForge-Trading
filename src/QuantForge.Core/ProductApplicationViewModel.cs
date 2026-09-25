namespace QuantForge.Core;

public enum ProductWorkspaceSection
{
    Research,
    Optimization,
    StrategyAudit,
    DataReliability,
    Reports
}

public sealed record ProductApplicationJobState(
    string JobFingerprint,
    ProductUiJobState State,
    string? Message,
    string? EvidenceFingerprint,
    bool CanRetry);

public sealed record ProductApplicationViewModel(
    string WorkflowFingerprint,
    ProductWorkspaceSection ActiveSection,
    ResearchBatchMode Mode,
    int TotalRuns,
    int CompleteRuns,
    int DataBlockedRuns,
    int InvalidRuns,
    decimal? MinimumReliabilityScore,
    decimal? AverageReliabilityScore,
    bool HasBlockingDataIssues,
    bool ResearchCommandsEnabled,
    bool LiveAccountEnabled,
    bool CanSubmitOrders,
    bool CanChangeApplicationSettings,
    IReadOnlyList<ProductApplicationJobState> Jobs,
    IReadOnlyList<ProductUiOperation> AllowedOperations);

public static class ProductApplicationViewModelRules
{
    public static ProductApplicationViewModel Create(
        ProductUiWorkflowState workflow,
        ProductWorkspaceSection activeSection)
    {
        ArgumentNullException.ThrowIfNull(workflow);

        if (string.IsNullOrWhiteSpace(workflow.WorkflowFingerprint))
            throw new InvalidOperationException("Product application state requires workflow identity.");

        if (workflow.LiveAccountEnabled || workflow.CanSubmitOrders || workflow.CanChangeApplicationSettings)
            throw new InvalidOperationException("Product application state cannot accept live, order, or settings authority.");

        if (workflow.Runs is null || workflow.Runs.Count != workflow.TotalRuns)
            throw new InvalidOperationException("Product application state requires the complete workflow run collection.");

        if (workflow.Reliability is null)
            throw new InvalidOperationException("Product application state requires data reliability information.");

        if (workflow.TotalRuns < 1 ||
            workflow.CompleteRuns < 0 ||
            workflow.DataBlockedRuns < 0 ||
            workflow.InvalidRuns < 0 ||
            workflow.CompleteRuns + workflow.DataBlockedRuns + workflow.InvalidRuns != workflow.TotalRuns)
            throw new InvalidOperationException("Product application workflow counts are inconsistent.");

        foreach (var assessment in workflow.Reliability)
            DataReliabilityRules.Validate(assessment);

        var jobs = workflow.Runs
            .OrderBy(x => x.JobId, StringComparer.Ordinal)
            .Select(CreateJobState)
            .ToArray();

        var completeCount = jobs.Count(x => x.State == ProductUiJobState.Complete);
        var blockedCount = jobs.Count(x => x.State == ProductUiJobState.DataBlocked);
        var invalidCount = jobs.Count(x => x.State == ProductUiJobState.Invalid);

        if (completeCount != workflow.CompleteRuns ||
            blockedCount != workflow.DataBlockedRuns ||
            invalidCount != workflow.InvalidRuns)
            throw new InvalidOperationException("Product application run states do not match workflow counts.");

        var hasBlockingDataIssues = workflow.Reliability.Any(x => !DataReliabilityRules.IsResearchAdmissible(x));
        var allowedOperations = GetAllowedOperations(activeSection);

        return new ProductApplicationViewModel(
            workflow.WorkflowFingerprint,
            activeSection,
            workflow.Mode,
            workflow.TotalRuns,
            workflow.CompleteRuns,
            workflow.DataBlockedRuns,
            workflow.InvalidRuns,
            workflow.MinimumReliabilityScore,
            workflow.AverageReliabilityScore,
            hasBlockingDataIssues,
            ResearchCommandsEnabled: !hasBlockingDataIssues,
            LiveAccountEnabled: false,
            CanSubmitOrders: false,
            CanChangeApplicationSettings: false,
            jobs,
            allowedOperations);
    }

    private static ProductApplicationJobState CreateJobState(ProductUiRunState run)
    {
        if (string.IsNullOrWhiteSpace(run.JobId) ||
            string.IsNullOrWhiteSpace(run.DatasetFingerprint) ||
            string.IsNullOrWhiteSpace(run.StrategyFingerprint))
            throw new InvalidOperationException("Product application job state requires complete research identity.");

        return run.Status switch
        {
            ResearchResultStatus.Complete when !string.IsNullOrWhiteSpace(run.EvidenceFingerprint) =>
                new(run.JobId, ProductUiJobState.Complete, null, run.EvidenceFingerprint, false),
            ResearchResultStatus.Complete =>
                throw new InvalidOperationException("A completed product application job requires execution evidence."),
            ResearchResultStatus.DataBlocked when !string.IsNullOrWhiteSpace(run.Message) =>
                new(run.JobId, ProductUiJobState.DataBlocked, run.Message, null, true),
            ResearchResultStatus.DataBlocked =>
                throw new InvalidOperationException("A data-blocked product application job requires an explicit reason."),
            ResearchResultStatus.Invalid when !string.IsNullOrWhiteSpace(run.Message) =>
                new(run.JobId, ProductUiJobState.Invalid, run.Message, null, true),
            ResearchResultStatus.Invalid =>
                throw new InvalidOperationException("An invalid product application job requires an explicit reason."),
            _ => throw new InvalidOperationException("Unknown product application research result state.")
        };
    }

    private static IReadOnlyList<ProductUiOperation> GetAllowedOperations(ProductWorkspaceSection section) =>
        section switch
        {
            ProductWorkspaceSection.Research => new[]
            {
                ProductUiOperation.StartResearch,
                ProductUiOperation.ViewResearchSummary,
                ProductUiOperation.ExportResearchReport
            },
            ProductWorkspaceSection.Optimization => new[]
            {
                ProductUiOperation.StartOptimization,
                ProductUiOperation.ViewResearchSummary,
                ProductUiOperation.ExportResearchReport
            },
            ProductWorkspaceSection.StrategyAudit => new[]
            {
                ProductUiOperation.AuditStrategy
            },
            ProductWorkspaceSection.DataReliability => new[]
            {
                ProductUiOperation.ImportMarketData,
                ProductUiOperation.ViewResearchSummary
            },
            ProductWorkspaceSection.Reports => new[]
            {
                ProductUiOperation.ViewResearchSummary,
                ProductUiOperation.ExportResearchReport
            },
            _ => throw new InvalidOperationException("Unknown product workspace section.")
        };
}
