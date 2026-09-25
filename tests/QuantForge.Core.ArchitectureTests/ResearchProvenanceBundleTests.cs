using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public class ResearchProvenanceBundleTests
{
    [Fact]
    public void Publishable_provenance_requires_sanitized_strategy_identity_match()
    {
        var (job, report) = Valid();
        var provenance = Source("wrong-sha");

        Assert.Throws<InvalidOperationException>(() =>
            ResearchProvenanceRules.Bind(job, provenance, report));
    }

    [Fact]
    public void Publishable_provenance_binds_job_dataset_strategy_and_evidence()
    {
        var (job, report) = Valid();
        var bundle = ResearchProvenanceRules.Bind(job, Source("strategy-sha"), report);

        Assert.Equal("data-sha", bundle.DatasetFingerprint);
        Assert.Equal("job-sha", bundle.JobFingerprint);
        Assert.Equal(report.EvidenceTail!.Value.Fingerprint, bundle.EvidenceFingerprint);
    }

    private static ProvenanceRecord Source(string sanitized)
        => new("artifact", "local://strategy", "2026-01-01T00:00:00Z", "source-sha", "original-sha", sanitized, "scrub-1");

    private static (ResearchJobSpec Job, ResearchReport Report) Valid()
    {
        var data = new DataAdmission("dataset", "data-sha", "TEST", "1m",
            DateTimeOffset.Parse("2026-01-01T00:00:00Z"), DateTimeOffset.Parse("2026-01-02T00:00:00Z"), true, true);
        var strategy = new StrategyCapabilityManifest("s1", "strategy-sha", false, false, false, false, false, false, false);
        var identity = new ResearchJobIdentity("data-sha", "strategy-sha", "exec-sha", "params-sha", "partition", "job-sha");
        var job = new ResearchJobSpec(AuthorityDomain.SimulatedAccount, data, strategy, identity, ExecutionTimingPolicy.NextBarOpen, false);
        var evidence = ResearchEvidence.CreateRoot(identity);
        var account = new AccountSnapshot("batch|s1|account", 1000m, 0m, 0m, 1000m, 0m);
        var report = new ResearchReport("job-sha", ResearchResultStatus.Complete, "data-sha", "strategy-sha", "exec-sha", "params-sha", "partition", null, account, evidence);
        return (job, report);
    }
}
