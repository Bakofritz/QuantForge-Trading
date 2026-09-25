using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public class DataReliabilityWorkflowTests
{
    [Fact]
    public void Workflow_runs_reliable_data_and_exposes_terminal_component_state()
    {
        var run = Request("s1", "strategy-1", "job-1", "data-sha");
        var reliability = new DataReliabilityAssessment("data-sha", 98m, true, 0, 0, null);

        var result = ResearchWorkflowCoordinator.Run(new ResearchWorkflowRequest(
            new ResearchBatchRequest(ResearchBatchMode.ReadOnlyResearch, new[] { run }),
            new[] { reliability }));

        var report = Assert.Single(result.Reports);
        Assert.Equal(ResearchResultStatus.Complete, report.Status);
        Assert.NotNull(report.Account);
        Assert.NotNull(report.EvidenceTail);
        Assert.Equal(ResearchComponentState.Complete, Assert.Single(result.ComponentStatuses).State);
        Assert.Equal(98m, Assert.Single(result.Reliability).ScorePercent);
    }

    [Fact]
    public void Workflow_blocks_unresolved_gaps_without_creating_performance_state()
    {
        var run = Request("s1", "strategy-1", "job-1", "data-sha");
        var reliability = new DataReliabilityAssessment(
            "data-sha", 72m, true, 2, 0, "Two source intervals are missing and were not inferred.");

        var result = ResearchWorkflowCoordinator.Run(new ResearchWorkflowRequest(
            new ResearchBatchRequest(ResearchBatchMode.ReadOnlyResearch, new[] { run }),
            new[] { reliability }));

        var report = Assert.Single(result.Reports);
        Assert.Equal(ResearchResultStatus.DataBlocked, report.Status);
        Assert.Null(report.Account);
        Assert.Null(report.EvidenceTail);
        Assert.Contains("unresolved data gap", report.BlockReason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Workflow_contains_unsafe_strategy_without_suppressing_valid_sibling()
    {
        var safe = Request("s1", "strategy-1", "job-1", "data-sha");
        var unsafeRun = Request("s2", "strategy-2", "job-2", "data-sha") with
        {
            Job = Request("s2", "strategy-2", "job-2", "data-sha").Job with
            {
                Strategy = Request("s2", "strategy-2", "job-2", "data-sha").Job.Strategy with { CanSubmitOrders = true }
            }
        };
        var reliability = new DataReliabilityAssessment("data-sha", 98m, true, 0, 0, null);

        var result = ResearchWorkflowCoordinator.Run(new ResearchWorkflowRequest(
            new ResearchBatchRequest(ResearchBatchMode.ReadOnlyOptimization, new[] { safe, unsafeRun }),
            new[] { reliability }));

        Assert.Equal(ResearchResultStatus.Complete, result.Reports[0].Status);
        Assert.Equal(ResearchResultStatus.Invalid, result.Reports[1].Status);
        Assert.Contains("order", result.Reports[1].BlockReason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Reliability_without_live_benchmark_requires_explicit_limitation_and_blocks_research()
    {
        var run = Request("s1", "strategy-1", "job-1", "data-sha");
        var reliability = new DataReliabilityAssessment(
            "data-sha", 85m, false, 0, 0, "Live benchmark unavailable for this dataset revision.");

        var result = ResearchWorkflowCoordinator.Run(new ResearchWorkflowRequest(
            new ResearchBatchRequest(ResearchBatchMode.ReadOnlyResearch, new[] { run }),
            new[] { reliability }));

        Assert.Equal(ResearchResultStatus.DataBlocked, Assert.Single(result.Reports).Status);
    }

    private static ResearchRunRequest Request(
        string strategyId,
        string strategyFingerprint,
        string jobId,
        string datasetFingerprint)
    {
        var time = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var data = new DataAdmission(
            "dataset", datasetFingerprint, "TEST", "1m", time.AddHours(-1), time.AddHours(1), true, true);
        var strategy = new StrategyCapabilityManifest(
            strategyId, strategyFingerprint, false, false, false, false, false, false, false);
        var identity = new ResearchJobIdentity(
            datasetFingerprint, strategyFingerprint, "exec-sha", "params-sha-" + strategyId, "partition", jobId);
        var job = new ResearchJobSpec(
            AuthorityDomain.SimulatedAccount, data, strategy, identity, ExecutionTimingPolicy.NextBarOpen, false);
        var ledger = $"batch|{strategyId}|account";
        var intent = new SimulationIntent(
            strategyId, ledger, SimulationSide.Buy, SimulationIntentType.Market, 1m, time, time.AddMinutes(1));
        var market = new MarketEvent(1, time.AddMinutes(1), 100m, 100m, 100m, 100m, 10m);
        var admission = new StrategyAdmissionEnvelope(
            StrategyAdmissionState.Admitted,
            strategy,
            new StrategyFeatureSelection(new[] { StrategyFeature.SignalGeneration }),
            "quarantine-" + strategyFingerprint,
            strategyFingerprint);

        return new ResearchRunRequest(job, new[] { intent }, new[] { market }, 1000m, 0m, 0m, admission);
    }
}
