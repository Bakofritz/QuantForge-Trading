using QuantForge.Core;
using Xunit;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchGateBundleFileStoreTests
{
    [Fact]
    public void Bundle_round_trips_and_storage_key_is_verified()
    {
        var root = Path.Combine(Path.GetTempPath(), "qf-gates-" + Guid.NewGuid().ToString("N"));
        try
        {
            var bundle = Bundle();
            var store = new ResearchGateBundleFileStore(root);
            store.Save(bundle);
            Assert.Equal(bundle, store.Load(bundle.BundleFingerprint));
            Assert.ThrowsAny<Exception>(() => store.Load("OTHER"));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    private static ResearchGateBundle Bundle()
    {
        const string fp = "DATASET123";
        var dataset = new DatasetCatalogEntry("dataset-1", fp, "MES", "1m",
            DateTimeOffset.Parse("2026-01-01T00:00:00Z"), DateTimeOffset.Parse("2026-01-01T00:01:00Z"), true,
            new ProvenanceRecord("artifact", "file://dataset", "2026-01-01T00:00:00Z", fp, fp, fp, "scrub-1"), fp);
        var coverage = SessionCoverageArtifactRules.Create(new SessionCoverageReport(fp, "POLICY1", 1, 1, 0, 0,
            Array.Empty<DateTimeOffset>(), Array.Empty<DateTimeOffset>(), true, "Explicit authoritative policy."));
        var reliability = new DataReliabilityAssessment(fp, 100m, true, 0, 0, null);
        var strategy = StrategyAdmissionArtifactRules.Create(new StrategyAdmissionEnvelope(
            StrategyAdmissionState.Admitted,
            new StrategyCapabilityManifest("strategy-1", "STRATEGY123", false, false, false, false, false, false, false),
            new StrategyFeatureSelection(new[] { StrategyFeature.SignalGeneration }), "QUARANTINE123", "STRATEGY123"));
        return ResearchGateBundleRules.Create(dataset, coverage, reliability, strategy);
    }
}
