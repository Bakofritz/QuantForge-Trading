using QuantForge.Core;
using Xunit;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchLaunchIntentTests
{
    [Fact]
    public void Launch_intent_binds_job_to_gate_bundle()
    {
        var gates = TestFixtures.Gates();
        var job = TestFixtures.Job(gates);
        var intent = ResearchLaunchIntentRules.Create(gates, job);
        ResearchLaunchIntentRules.Validate(intent, gates);
    }

    [Fact]
    public void Strategy_mismatch_is_rejected()
    {
        var gates = TestFixtures.Gates();
        var job = TestFixtures.Job(gates) with { Strategy = gates.Strategy.Envelope.Manifest with { StrategyId = "other" } };
        Assert.Throws<InvalidOperationException>(() => ResearchLaunchIntentRules.Create(gates, job));
    }
}

internal static class TestFixtures
{
    public static ResearchGateBundle Gates()
    {
        const string fp = "DATASET123";
        var dataset = new DatasetCatalogEntry("dataset-1", fp, "MES", "1m", DateTimeOffset.Parse("2026-01-01T00:00:00Z"), DateTimeOffset.Parse("2026-01-01T00:01:00Z"), true,
            new ProvenanceRecord("artifact", "file://dataset", "2026-01-01T00:00:00Z", fp, fp, fp, "scrub-1"), fp);
        var coverage = SessionCoverageArtifactRules.Create(new SessionCoverageReport(fp, "POLICY1", 1, 1, 0, 0, Array.Empty<DateTimeOffset>(), Array.Empty<DateTimeOffset>(), true, "Explicit authoritative policy."));
        var reliability = new DataReliabilityAssessment(fp, 100m, true, 0, 0, null);
        var strategy = StrategyAdmissionArtifactRules.Create(new StrategyAdmissionEnvelope(StrategyAdmissionState.Admitted,
            new StrategyCapabilityManifest("strategy-1", "STRATEGY123", false, false, false, false, false, false, false),
            new StrategyFeatureSelection(new[] { StrategyFeature.SignalGeneration }), "QUARANTINE123", "STRATEGY123"));
        return ResearchGateBundleRules.Create(dataset, coverage, reliability, strategy);
    }

    public static ResearchJobSpec Job(ResearchGateBundle gates)
    {
        var data = new DatasetCatalog().Tap(c => c.Register(gates.Dataset)).RequireAdmission(gates.Dataset.DatasetId);
        var identity = new ResearchJobIdentity(gates.Dataset.DatasetFingerprint, gates.Strategy.Envelope.Manifest.SourceFingerprint, "EXEC1", "PARAM1", "PART1", "JOB1");
        return new ResearchJobSpec(AuthorityDomain.ReadOnlyResearch, data, gates.Strategy.Envelope.Manifest, identity,
            ExecutionTimingPolicy.NextBarOpen, false);
    }

    private static T Tap<T>(this T value, Action<T> action) { action(value); return value; }
}
