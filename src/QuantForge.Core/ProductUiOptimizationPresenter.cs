namespace QuantForge.Core;

public sealed record ProductUiOptimizationVariantState(
    string JobFingerprint,
    string DatasetFingerprint,
    string StrategyFingerprint,
    string ParameterSetFingerprint,
    string TemporalPartitionId,
    string LedgerNamespace);

public sealed record ProductUiOptimizationState(
    int VariantCount,
    bool LiveAccountEnabled,
    bool CanSubmitOrders,
    bool CanChangeApplicationSettings,
    IReadOnlyList<ProductUiOptimizationVariantState> Variants);

public static class ProductUiOptimizationPresenter
{
    public static ProductUiOptimizationState Create(ResearchOptimizationPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var validated = ResearchOptimizationPlanRules.Create(plan.Variants);
        var variants = validated.Variants
            .OrderBy(x => x.Job.Identity.JobFingerprint, StringComparer.Ordinal)
            .Select(request => new ProductUiOptimizationVariantState(
                request.Job.Identity.JobFingerprint,
                request.Job.Identity.DatasetFingerprint,
                request.Job.Identity.StrategyFingerprint,
                request.Job.Identity.ParameterSetFingerprint,
                request.Job.Identity.TemporalPartitionId,
                request.Intents[0].LedgerNamespace))
            .ToArray();

        return new ProductUiOptimizationState(
            variants.Length,
            LiveAccountEnabled: false,
            CanSubmitOrders: false,
            CanChangeApplicationSettings: false,
            variants);
    }
}

public sealed record ProductUiOptimizationResultVariantState(
    string JobId,
    ResearchResultStatus Status,
    decimal? FinalEquity,
    decimal? RealizedPnl,
    bool Selected,
    string? Message);

public sealed record ProductUiOptimizationResultState(
    string ResultFingerprint,
    bool Complete,
    ResearchOptimizationObjective? Objective,
    string? SelectedJobId,
    bool LiveAccountEnabled,
    bool CanSubmitOrders,
    bool CanChangeApplicationSettings,
    IReadOnlyList<ProductUiOptimizationResultVariantState> Variants);

public static class ProductUiOptimizationResultPresenter
{
    public static ProductUiOptimizationResultState Create(ResearchOptimizationApplicationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(result.ResultFingerprint))
            throw new InvalidOperationException("Optimization result identity is required for application presentation.");
        if (result.Evaluation.Reports is null)
            throw new InvalidOperationException("Optimization result requires the complete variant report collection.");

        foreach (var report in result.Evaluation.Reports)
            ResearchReportRules.Validate(report);

        var duplicateJob = result.Evaluation.Reports
            .GroupBy(x => x.JobId, StringComparer.Ordinal)
            .FirstOrDefault(x => x.Count() > 1);
        if (duplicateJob is not null)
            throw new InvalidOperationException("Optimization result contains duplicate job identities.");

        if (result.Evaluation.Complete)
        {
            if (result.Evaluation.Reports.Count == 0 || result.Selection is null || result.Evaluation.BlockReason is not null)
                throw new InvalidOperationException("A complete optimization result requires variants, a deterministic selection, and no block reason.");
            var verified = ResearchOptimizationSelectionRules.Select(result.Evaluation, result.Selection.Objective);
            if (verified != result.Selection)
                throw new InvalidOperationException("Optimization selection does not match the deterministic result set.");
        }
        else
        {
            if (result.Selection is not null || string.IsNullOrWhiteSpace(result.Evaluation.BlockReason))
                throw new InvalidOperationException("A blocked optimization result cannot contain a selection and requires an explicit reason.");
        }

        var variants = result.Evaluation.Reports
            .OrderBy(x => x.JobId, StringComparer.Ordinal)
            .Select(report => new ProductUiOptimizationResultVariantState(
                report.JobId,
                report.Status,
                report.Account?.Equity,
                report.Account?.RealizedPnl,
                string.Equals(report.JobId, result.Selection?.SelectedJobId, StringComparison.Ordinal),
                report.BlockReason))
            .ToArray();

        return new ProductUiOptimizationResultState(
            result.ResultFingerprint,
            result.Evaluation.Complete,
            result.Selection?.Objective,
            result.Selection?.SelectedJobId,
            LiveAccountEnabled: false,
            CanSubmitOrders: false,
            CanChangeApplicationSettings: false,
            variants);
    }
}
