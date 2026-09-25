using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public class Phase2ExitContractTests
{
    [Fact]
    public void Phase2_research_chain_preserves_admission_authority_isolation_provenance_and_terminal_state()
    {
        var run1 = Request("s1", "strategy-1", "job-1");
        var run2 = Request("s2", "strategy-2", "job-2");

        var reports = ReadOnlyResearchOrchestrator.Run(new ResearchBatchRequest(
            ResearchBatchMode.ReadOnlyOptimization, new[] { run1, run2 }));

        Assert.Equal(2, reports.Count);
        Assert.All(reports, r => Assert.Equal(ResearchResultStatus.Complete, r.Status));
        Assert.NotEqual(reports[0].StrategyFingerprint, reports[1].StrategyFingerprint);

        var status = ResearchComponentStatusRules.FromReport(reports[0]);
        ResearchComponentStatusRules.RequireTerminal(status);
        Assert.Equal(ResearchComponentState.Complete, status.State);

        var provenance = new ProvenanceRecord(
            "artifact-1", "local://strategy-1", "2026-01-01T00:00:00Z",
            "source-sha", "original-sha", "strategy-1", "scrub-1");
        var bundle = ResearchProvenanceRules.Bind(run1.Job, provenance, reports[0]);

        Assert.Equal(run1.Job.Identity.DatasetFingerprint, bundle.DatasetFingerprint);
        Assert.Equal(run1.Job.Identity.JobFingerprint, bundle.JobFingerprint);
        Assert.False(string.IsNullOrWhiteSpace(bundle.EvidenceFingerprint));
    }

    [Fact]
    public void Phase2_chain_fails_closed_for_live_or_unsafe_strategy_authority()
    {
        var run = Request("s1", "strategy-1", "job-1");
        var unsafeRun = run with
        {
            Job = run.Job with
            {
                Strategy = run.Job.Strategy with { CanSubmitOrders = true }
            }
        };

        Assert.Throws<InvalidOperationException>(() =>
            ReadOnlyResearchOrchestrator.Run(new ResearchBatchRequest(
                ResearchBatchMode.ReadOnlyResearch, new[] { unsafeRun })));
    }

    private static ResearchRunRequest Request(string strategyId, string fingerprint, string jobId)
    {
        var time = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var data = new DataAdmission("dataset", "data-sha", "TEST", "1m", time.AddHours(-1), time.AddHours(1), true, true);
        var strategy = new StrategyCapabilityManifest(strategyId, fingerprint, false, false, false, false, false, false, false);
        var identity = new ResearchJobIdentity("data-sha", fingerprint, "exec-sha", "params-sha", "partition", jobId);
        var job = new ResearchJobSpec(AuthorityDomain.SimulatedAccount, data, strategy, identity, ExecutionTimingPolicy.NextBarOpen, false);
        var ledger = $"batch|{strategyId}|account";
        var intent = new SimulationIntent(strategyId, ledger, SimulationSide.Buy, SimulationIntentType.Market, 1m, time, time.AddMinutes(1));
        var market = new MarketEvent(1, time.AddMinutes(1), 100m, 100m, 100m, 100m, 10m);
        var admission = new StrategyAdmissionEnvelope(
            StrategyAdmissionState.Admitted, strategy,
            new StrategyFeatureSelection(new[] { StrategyFeature.SignalGeneration }),
            "quarantine-" + fingerprint, fingerprint);
        return new ResearchRunRequest(job, new[] { intent }, new[] { market }, 1000m, 0m, 0m, admission);
    }
}
