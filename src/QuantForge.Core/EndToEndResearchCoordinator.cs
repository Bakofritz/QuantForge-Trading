namespace QuantForge.Core;

public sealed record EndToEndResearchRequest(
    ResearchWorkflowRequest Workflow,
    DatasetCatalog DatasetCatalog,
    IReadOnlyList<StrategyAdmissionEnvelope> StrategyAdmissions);

public static class EndToEndResearchCoordinator
{
    public static ResearchWorkflowResult RunResearchAdmitted(EndToEndResearchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Workflow.SessionCoverage is null || request.Workflow.SessionCoverage.Count == 0)
            throw new InvalidOperationException("Research-admitted execution requires authoritative session coverage evidence.");

        foreach (var run in request.Workflow.Batch.Runs)
        {
            var dataset = request.DatasetCatalog.RequireAdmission(run.Job.Data.DatasetId);
            var coverage = request.Workflow.SessionCoverage.FirstOrDefault(x =>
                string.Equals(x.DatasetFingerprint, dataset.DatasetFingerprint, StringComparison.Ordinal));
            if (coverage is null || !coverage.ResearchAdmissible)
                throw new InvalidOperationException("Every research-admitted run requires authoritative, complete session coverage for its admitted dataset.");
        }

        return Run(request);
    }

    public static ResearchWorkflowResult Run(EndToEndResearchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.DatasetCatalog);
        ArgumentNullException.ThrowIfNull(request.StrategyAdmissions);

        var strategies = new Dictionary<string, StrategyAdmissionEnvelope>(StringComparer.Ordinal);
        foreach (var admission in request.StrategyAdmissions)
        {
            var manifest = StrategyAdmissionPipeline.RequireAdmitted(admission);
            if (!strategies.TryAdd(manifest.StrategyId, admission))
                throw new InvalidOperationException("An end-to-end workflow cannot contain duplicate strategy admissions.");
        }

        var runs = new List<ResearchRunRequest>(request.Workflow.Batch.Runs.Count);
        foreach (var run in request.Workflow.Batch.Runs)
        {
            var job = run.Job;
            var admittedData = request.DatasetCatalog.RequireAdmission(job.Data.DatasetId);
            if (admittedData != job.Data)
                throw new InvalidOperationException("Research job data admission does not exactly match the catalog entry.");

            if (!strategies.TryGetValue(job.Strategy.StrategyId, out var strategyAdmission))
                throw new InvalidOperationException("Every end-to-end research run requires an admitted strategy envelope.");

            var manifest = StrategyAdmissionPipeline.RequireAdmitted(strategyAdmission);
            if (!string.Equals(manifest.SourceFingerprint, job.Strategy.SourceFingerprint, StringComparison.Ordinal))
                throw new InvalidOperationException("Research job strategy fingerprint does not match the admitted strategy envelope.");

            runs.Add(run with { StrategyAdmission = strategyAdmission });
        }

        var workflow = request.Workflow with
        {
            Batch = request.Workflow.Batch with { Runs = runs }
        };

        return ResearchWorkflowCoordinator.Run(workflow);
    }
}
