namespace QuantForge.Core;

public enum ResearchBatchMode
{
    ReadOnlyResearch,
    ReadOnlyOptimization
}

public sealed record ResearchBatchRequest(
    ResearchBatchMode Mode,
    IReadOnlyList<ResearchRunRequest> Runs);

public static class ReadOnlyResearchOrchestrator
{
    public static IReadOnlyList<ResearchReport> Run(ResearchBatchRequest batch)
    {
        if (batch.Runs is null || batch.Runs.Count == 0)
            throw new InvalidOperationException("Read-only research orchestration requires at least one run.");

        foreach (var run in batch.Runs)
        {
            if (run.Job.Authority == AuthorityDomain.LiveAccount)
                throw new InvalidOperationException("Read-only research orchestration cannot use live-account authority.");

            if (run.Job.Strategy.CanSubmitOrders ||
                run.Job.Strategy.CanChangeApplicationSettings ||
                run.Job.Strategy.RequiresLiveAccount)
                throw new InvalidOperationException("Read-only research orchestration cannot grant order, settings, or live-account authority.");
        }

        var reports = new List<ResearchReport>(batch.Runs.Count);
        foreach (var run in batch.Runs)
        {
            try
            {
                reports.Add(DeterministicResearchRunner.Run(run));
            }
            catch (Exception ex)
            {
                var id = run.Job.Identity;
                reports.Add(new ResearchReport(
                    id.JobFingerprint,
                    ResearchResultStatus.Invalid,
                    id.DatasetFingerprint,
                    id.StrategyFingerprint,
                    id.ExecutionPolicyFingerprint,
                    id.ParameterSetFingerprint,
                    id.TemporalPartitionId,
                    $"Contained batch execution failure: {ex.Message}",
                    null,
                    null));
            }
        }

        return reports;
    }
}
