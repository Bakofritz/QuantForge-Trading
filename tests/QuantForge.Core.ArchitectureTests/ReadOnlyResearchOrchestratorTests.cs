using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public class ReadOnlyResearchOrchestratorTests
{
    [Fact]
    public void Orchestrator_runs_independent_strategies_without_crossing_ledgers()
    {
        var reports = ReadOnlyResearchOrchestrator.Run(new ResearchBatchRequest(
            ResearchBatchMode.ReadOnlyOptimization,
            new[] { Request("s1", "sha-1"), Request("s2", "sha-2") }));

        Assert.Equal(2, reports.Count);
        Assert.All(reports, report => Assert.Equal(ResearchResultStatus.Complete, report.Status));
        Assert.NotEqual(reports[0].StrategyFingerprint, reports[1].StrategyFingerprint);
    }

    [Fact]
    public void Orchestrator_rejects_order_capable_strategy()
    {
        var run = Request("s1", "sha-1");
        run = run with
        {
            Job = run.Job with
            {
                Strategy = run.Job.Strategy with { CanSubmitOrders = true }
            }
        };

        Assert.Throws<InvalidOperationException>(() =>
            ReadOnlyResearchOrchestrator.Run(new ResearchBatchRequest(
                ResearchBatchMode.ReadOnlyResearch, new[] { run })));
    }

    private static ResearchRunRequest Request(string strategyId, string fingerprint)
    {
        var time = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var data = new DataAdmission("dataset", "data-sha", "TEST", "1m", time.AddHours(-1), time.AddHours(1), true, true);
        var strategy = new StrategyCapabilityManifest(strategyId, fingerprint, false, false, false, false, false, false, false);
        var identity = new ResearchJobIdentity("data-sha", fingerprint, "exec-sha", "params-sha", "partition", "job-" + strategyId);
        var job = new ResearchJobSpec(AuthorityDomain.SimulatedAccount, data, strategy, identity, ExecutionTimingPolicy.NextBarOpen, false);
        var ledger = $"batch|{strategyId}|account";
        var intent = new SimulationIntent(strategyId, ledger, SimulationSide.Buy, SimulationIntentType.Market, 1m, time, time.AddMinutes(1));
        var market = new MarketEvent(1, time.AddMinutes(1), 100m, 100m, 100m, 100m, 10m);
        return new ResearchRunRequest(job, new[] { intent }, new[] { market }, 1000m, 0m, 0m);
    }
}
