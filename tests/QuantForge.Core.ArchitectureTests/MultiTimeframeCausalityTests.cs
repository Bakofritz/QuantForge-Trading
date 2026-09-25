using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public class MultiTimeframeCausalityTests
{
    [Fact]
    public void Workflow_accepts_only_closed_and_available_multi_timeframe_observations()
    {
        var run = Request();
        var decision = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var context = new ResearchTimeframeContext(
            "job-1",
            decision,
            new[]
            {
                new TimeframeObservation("1m", decision.AddMinutes(-2), decision.AddMinutes(-1), decision.AddMinutes(-1)),
                new TimeframeObservation("5m", decision.AddMinutes(-10), decision.AddMinutes(-5), decision.AddMinutes(-5))
            });

        var result = Run(run, context);

        Assert.Equal(ResearchResultStatus.Complete, Assert.Single(result.Reports).Status);
    }

    [Fact]
    public void Workflow_rejects_unfinished_higher_timeframe_bar_as_forward_information()
    {
        var run = Request();
        var decision = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var context = new ResearchTimeframeContext(
            "job-1",
            decision,
            new[]
            {
                new TimeframeObservation("1m", decision.AddMinutes(-2), decision.AddMinutes(-1), decision.AddMinutes(-1)),
                new TimeframeObservation("5m", decision.AddMinutes(-1), decision.AddMinutes(4), decision.AddMinutes(4))
            });

        var result = Run(run, context);
        var report = Assert.Single(result.Reports);

        Assert.Equal(ResearchResultStatus.Invalid, report.Status);
        Assert.Null(report.Account);
        Assert.Null(report.EvidenceTail);
        Assert.Contains("unfinished bar", report.BlockReason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Workflow_rejects_information_that_was_not_available_at_decision_time()
    {
        var run = Request();
        var decision = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var context = new ResearchTimeframeContext(
            "job-1",
            decision,
            new[]
            {
                new TimeframeObservation("5m", decision.AddMinutes(-10), decision.AddMinutes(-5), decision.AddSeconds(1))
            });

        var result = Run(run, context);
        var report = Assert.Single(result.Reports);

        Assert.Equal(ResearchResultStatus.Invalid, report.Status);
        Assert.Contains("information timestamp", report.BlockReason, StringComparison.OrdinalIgnoreCase);
    }

    private static ResearchWorkflowResult Run(
        ResearchRunRequest run,
        ResearchTimeframeContext context)
    {
        var reliability = new DataReliabilityAssessment("data-sha", 98m, true, 0, 0, null);
        return ResearchWorkflowCoordinator.Run(new ResearchWorkflowRequest(
            new ResearchBatchRequest(ResearchBatchMode.ReadOnlyResearch, new[] { run }),
            new[] { reliability },
            new[] { context }));
    }

    private static ResearchRunRequest Request()
    {
        var signal = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var data = new DataAdmission(
            "dataset", "data-sha", "TEST", "1m", signal.AddHours(-1), signal.AddHours(1), true, true);
        var strategy = new StrategyCapabilityManifest(
            "s1", "strategy-1", false, false, false, false, false, false, false);
        var identity = new ResearchJobIdentity(
            "data-sha", "strategy-1", "exec-sha", "params-sha", "partition", "job-1");
        var job = new ResearchJobSpec(
            AuthorityDomain.SimulatedAccount, data, strategy, identity, ExecutionTimingPolicy.NextBarOpen, false);
        var intent = new SimulationIntent(
            "s1", "batch|s1|account", SimulationSide.Buy, SimulationIntentType.Market, 1m, signal, signal.AddMinutes(1));
        var market = new MarketEvent(1, signal.AddMinutes(1), 100m, 100m, 100m, 100m, 10m);
        var admission = new StrategyAdmissionEnvelope(
            StrategyAdmissionState.Admitted,
            strategy,
            new StrategyFeatureSelection(new[] { StrategyFeature.SignalGeneration }),
            "quarantine-strategy-1",
            "strategy-1");

        return new ResearchRunRequest(job, new[] { intent }, new[] { market }, 1000m, 0m, 0m, admission);
    }
}
