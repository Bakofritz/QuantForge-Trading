namespace QuantForge.Core;

public sealed record ResearchApplicationWorkflowRequest(
    EndToEndResearchRequest EndToEnd,
    IReadOnlyDictionary<string, ProvenanceRecord> StrategySources);

public sealed record ResearchApplicationWorkflowResult(
    ResearchWorkflowSummary Summary,
    IReadOnlyList<ResearchPublicationArtifact> Publications);

public static class ResearchApplicationWorkflow
{
    public static ResearchApplicationWorkflowResult Run(ResearchApplicationWorkflowRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.StrategySources);

        var result = EndToEndResearchCoordinator.RunResearchAdmitted(request.EndToEnd);
        var summary = ResearchWorkflowSummaryFactory.Create(request.EndToEnd.Workflow.Batch.Mode, result);
        var publications = new List<ResearchPublicationArtifact>();

        var reliability = result.Reliability.ToDictionary(x => x.DatasetFingerprint, StringComparer.Ordinal);
        var coverage = request.EndToEnd.Workflow.SessionCoverage!.ToDictionary(x => x.DatasetFingerprint, StringComparer.Ordinal);
        foreach (var report in result.Reports.Where(x => x.Status == ResearchResultStatus.Complete))
        {
            var run = request.EndToEnd.Workflow.Batch.Runs.Single(x => x.Job.Identity.JobFingerprint == report.JobId);
            if (!request.StrategySources.TryGetValue(run.Job.Strategy.StrategyId, out var strategySource))
                throw new InvalidOperationException("A complete publication requires provenance for its admitted strategy.");
            if (!reliability.TryGetValue(report.DatasetFingerprint, out var assessment) ||
                !coverage.TryGetValue(report.DatasetFingerprint, out var session))
                throw new InvalidOperationException("A complete publication requires bound reliability and session evidence.");

            var publication = ResearchPublicationRules.Bind(run.Job, strategySource, report, assessment);
            var evidence = ResearchPublicationEvidenceRules.Bind(publication, session);
            var output = ResearchResultPipeline.FromReport(report);
            publications.Add(ResearchPublicationArtifactFactory.Create(evidence, output));
        }

        return new ResearchApplicationWorkflowResult(summary, publications);
    }
}
