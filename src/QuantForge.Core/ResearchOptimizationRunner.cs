namespace QuantForge.Core;

public sealed record ResearchOptimizationResult(
    IReadOnlyList<ResearchReport> Reports,
    bool Complete,
    string? BlockReason);

public static class ResearchOptimizationRunner
{
    public static ResearchOptimizationResult Run(ResearchOptimizationPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var validated = ResearchOptimizationPlanRules.Create(plan.Variants);
        var reports = new List<ResearchReport>(validated.Variants.Count);
        foreach (var variant in validated.Variants)
            reports.Add(ResearchResultPipeline.Run(variant).Report);

        var blocked = reports.FirstOrDefault(x => x.Status != ResearchResultStatus.Complete);
        return blocked.Status != ResearchResultStatus.Complete
            ? new ResearchOptimizationResult(reports, false, $"Optimization blocked by variant {blocked.JobId}: {blocked.BlockReason ?? blocked.Status.ToString()}.")
            : new ResearchOptimizationResult(reports, true, null);
    }
}
