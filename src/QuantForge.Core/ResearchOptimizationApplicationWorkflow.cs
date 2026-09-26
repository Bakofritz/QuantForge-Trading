namespace QuantForge.Core;

public sealed record ResearchOptimizationApplicationRequest(
    ResearchOptimizationPlan Plan,
    DatasetCatalog DatasetCatalog,
    IReadOnlyList<StrategyAdmissionEnvelope> StrategyAdmissions,
    IReadOnlyList<DataReliabilityAssessment> Reliability,
    IReadOnlyList<SessionCoverageReport> SessionCoverage);

public sealed record ResearchOptimizationApplicationResult(
    ResearchOptimizationResult Evaluation,
    ResearchOptimizationSelection? Selection,
    string ResultFingerprint);

public static class ResearchOptimizationApplicationWorkflow
{
    public static ResearchOptimizationApplicationResult Run(
        ResearchOptimizationApplicationRequest request,
        ResearchOptimizationObjective objective)
    {
        ArgumentNullException.ThrowIfNull(request);
        var plan = ResearchOptimizationPlanRules.Create(request.Plan.Variants);
        var strategies = request.StrategyAdmissions
            .Select(StrategyAdmissionPipeline.RequireAdmitted)
            .ToDictionary(x => x.StrategyId, StringComparer.Ordinal);
        var reliability = request.Reliability.ToDictionary(x => x.DatasetFingerprint, StringComparer.Ordinal);
        var coverage = request.SessionCoverage.ToDictionary(x => x.DatasetFingerprint, StringComparer.Ordinal);

        foreach (var variant in plan.Variants)
        {
            var data = request.DatasetCatalog.RequireAdmission(variant.Job.Data.DatasetId);
            if (data != variant.Job.Data)
                throw new InvalidOperationException("Optimization variant data admission does not exactly match the catalog entry.");
            if (!strategies.TryGetValue(variant.Job.Strategy.StrategyId, out var strategy) ||
                !string.Equals(strategy.SourceFingerprint, variant.Job.Strategy.SourceFingerprint, StringComparison.Ordinal))
                throw new InvalidOperationException("Optimization variant requires a matching admitted strategy.");
            if (!reliability.TryGetValue(variant.Job.Identity.DatasetFingerprint, out var assessment) ||
                !DataReliabilityRules.IsResearchAdmissible(assessment))
                throw new InvalidOperationException("Optimization requires research-admissible data reliability for every variant dataset.");
            if (!coverage.TryGetValue(variant.Job.Identity.DatasetFingerprint, out var session) ||
                !session.ResearchAdmissible)
                throw new InvalidOperationException("Optimization requires authoritative complete session coverage for every variant dataset.");
        }

        var evaluation = ResearchOptimizationRunner.Run(plan);
        var selection = evaluation.Complete
            ? ResearchOptimizationSelectionRules.Select(evaluation, objective)
            : null;
        var fingerprint = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(string.Join("\n", new[] {
                objective.ToString(),
                selection?.SelectionFingerprint ?? "blocked",
                string.Join("|", evaluation.Reports.Select(x => $"{x.JobId}:{x.Status}:{x.EvidenceTail?.Fingerprint ?? x.BlockReason}"))
            }))));
        return new ResearchOptimizationApplicationResult(evaluation, selection, fingerprint);
    }
}
