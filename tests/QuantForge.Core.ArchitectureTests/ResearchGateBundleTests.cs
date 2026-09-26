using QuantForge.Core;
using Xunit;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchGateBundleTests
{
    [Fact]
    public void Bundle_binds_dataset_session_reliability_and_strategy()
    {
        var dataset = Dataset();
        var coverage = Coverage(dataset.DatasetFingerprint);
        var reliability = new DataReliabilityAssessment(dataset.DatasetFingerprint, 100m, true, 0, 0, null);
        var strategy = StrategyAdmissionArtifactRules.Create(Strategy());
        var bundle = ResearchGateBundleRules.Create(dataset, coverage, reliability, strategy);
        ResearchGateBundleRules.Validate(bundle);
    }

    [Fact]
    public void Mismatched_reliability_dataset_fails_closed()
    {
        var dataset = Dataset();
        var reliability = new DataReliabilityAssessment("OTHER", 100m, true, 0, 0, null);
        Assert.Throws<InvalidOperationException>(() => ResearchGateBundleRules.Create(
            dataset, Coverage(dataset.DatasetFingerprint), reliability, StrategyAdmissionArtifactRules.Create(Strategy())));
    }

    private static DatasetCatalogEntry Dataset()
    {
        const string fp = "DATASET123";
        return new DatasetCatalogEntry("dataset-1", fp, "MES", "1m",
            DateTimeOffset.Parse("2026-01-01T00:00:00Z"), DateTimeOffset.Parse("2026-01-01T00:01:00Z"), true,
            new ProvenanceRecord("artifact", "file://dataset", "2026-01-01T00:00:00Z", fp, fp, fp, "scrub-1"), fp);
    }

    private static SessionCoverageArtifact Coverage(string datasetFingerprint)
    {
        var report = new SessionCoverageReport(datasetFingerprint, "POLICY1", 1, 1, 0, 0,
            Array.Empty<DateTimeOffset>(), Array.Empty<DateTimeOffset>(), true, "Explicit authoritative policy.");
        return SessionCoverageArtifactRules.Create(report);
    }

    private static StrategyAdmissionEnvelope Strategy() => new(
        StrategyAdmissionState.Admitted,
        new StrategyCapabilityManifest("strategy-1", "STRATEGY123", false, false, false, false, false, false, false),
        new StrategyFeatureSelection(new[] { StrategyFeature.SignalGeneration }), "QUARANTINE123", "STRATEGY123");
}
