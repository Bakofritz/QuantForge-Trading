using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public class ResearchPublicationBundleTests
{
    [Fact]
    public void Complete_result_binds_provenance_and_reliability_for_publication()
    {
        var run = Request();
        var report = DeterministicResearchRunner.Run(run);
        var source = Source();
        var reliability = new DataReliabilityAssessment("data-sha", 98m, true, 0, 0, null);

        var bundle = ResearchPublicationRules.Bind(run.Job, source, report, reliability);

        Assert.Equal("job-1", bundle.Report.JobId);
        Assert.Equal("data-sha", bundle.Reliability.DatasetFingerprint);
        Assert.Equal(report.EvidenceTail!.Value.Fingerprint, bundle.Provenance.EvidenceFingerprint);
    }

    [Fact]
    public void Publication_rejects_unresolved_data_gaps()
    {
        var run = Request();
        var report = DeterministicResearchRunner.Run(run);
        var reliability = new DataReliabilityAssessment(
            "data-sha", 70m, true, 1, 0, "One interval is unresolved and was not inferred.");

        Assert.Throws<InvalidOperationException>(() =>
            ResearchPublicationRules.Bind(run.Job, Source(), report, reliability));
    }

    [Fact]
    public void Publication_rejects_reliability_for_a_different_dataset()
    {
        var run = Request();
        var report = DeterministicResearchRunner.Run(run);
        var reliability = new DataReliabilityAssessment("other-data", 98m, true, 0, 0, null);

        Assert.Throws<InvalidOperationException>(() =>
            ResearchPublicationRules.Bind(run.Job, Source(), report, reliability));
    }

    private static ProvenanceRecord Source() =>
        new(
            "artifact-1",
            "local://strategy-1",
            "2026-01-01T00:00:00Z",
            "source-sha",
            "original-sha",
            "strategy-1",
            "scrub-1");

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
