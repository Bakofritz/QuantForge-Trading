using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public class ResearchWorkflowSummaryTests
{
    [Fact]
    public void Summary_is_deterministic_and_counts_terminal_results()
    {
        var result = Workflow(98m);

        var first = ResearchWorkflowSummaryFactory.Create(ResearchBatchMode.ReadOnlyResearch, result);
        var second = ResearchWorkflowSummaryFactory.Create(ResearchBatchMode.ReadOnlyResearch, result);

        Assert.Equal(first.WorkflowFingerprint, second.WorkflowFingerprint);
        Assert.Equal(1, first.TotalRuns);
        Assert.Equal(1, first.CompleteRuns);
        Assert.Equal(0, first.DataBlockedRuns);
        Assert.Equal(0, first.InvalidRuns);
        Assert.Equal(98m, first.MinimumReliabilityScore);
        Assert.Equal(98m, first.AverageReliabilityScore);
    }

    [Fact]
    public void Reliability_change_changes_workflow_fingerprint()
    {
        var first = ResearchWorkflowSummaryFactory.Create(
            ResearchBatchMode.ReadOnlyResearch,
            Workflow(98m));
        var second = ResearchWorkflowSummaryFactory.Create(
            ResearchBatchMode.ReadOnlyResearch,
            Workflow(97m));

        Assert.NotEqual(first.WorkflowFingerprint, second.WorkflowFingerprint);
    }

    [Fact]
    public void Markdown_export_contains_reproducibility_identity_and_reliability()
    {
        var summary = ResearchWorkflowSummaryFactory.Create(
            ResearchBatchMode.ReadOnlyResearch,
            Workflow(98m));

        var markdown = ResearchWorkflowMarkdownExporter.Export(summary);

        Assert.Contains("QuantForge Research Workflow Report", markdown, StringComparison.Ordinal);
        Assert.Contains(summary.WorkflowFingerprint, markdown, StringComparison.Ordinal);
        Assert.Contains("job-1", markdown, StringComparison.Ordinal);
        Assert.Contains("98%", markdown, StringComparison.Ordinal);
    }

    private static ResearchWorkflowResult Workflow(decimal score)
    {
        var run = Request();
        var reliability = new DataReliabilityAssessment("data-sha", score, true, 0, 0, null);
        return ResearchWorkflowCoordinator.Run(new ResearchWorkflowRequest(
            new ResearchBatchRequest(ResearchBatchMode.ReadOnlyResearch, new[] { run }),
            new[] { reliability }));
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
