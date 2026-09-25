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
