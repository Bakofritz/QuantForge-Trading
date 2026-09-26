using QuantForge.Core;
using Xunit;

namespace QuantForge.Core.ArchitectureTests;

public sealed class StrategyAdmissionArtifactTests
{
    [Fact]
    public void Admitted_strategy_round_trips_with_exact_fingerprint()
    {
        var envelope = Envelope();
        var artifact = StrategyAdmissionArtifactRules.Create(envelope);
        var root = Path.Combine(Path.GetTempPath(), "qf-strategy-admission-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new StrategyAdmissionArtifactFileStore(root);
            store.Save(artifact);
            var loaded = store.Load(artifact.ArtifactFingerprint);
            Assert.Equal(artifact, loaded);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void Non_admitted_strategy_cannot_be_persisted()
    {
        var envelope = Envelope() with { State = StrategyAdmissionState.Sanitized };
        Assert.Throws<InvalidOperationException>(() => StrategyAdmissionArtifactRules.Create(envelope));
    }

    [Fact]
    public void Tampered_identity_fails_validation()
    {
        var artifact = StrategyAdmissionArtifactRules.Create(Envelope());
        Assert.Throws<InvalidOperationException>(() => StrategyAdmissionArtifactRules.Validate(artifact with { ArtifactFingerprint = "BAD" }));
    }

    private static StrategyAdmissionEnvelope Envelope()
    {
        const string sha = "ABCDEF0123456789";
        return new StrategyAdmissionEnvelope(
            StrategyAdmissionState.Admitted,
            new StrategyCapabilityManifest("strategy-1", sha, false, false, false, false, false, false, false),
            new StrategyFeatureSelection(new[] { StrategyFeature.SignalGeneration, StrategyFeature.PositionSizing }),
            "QUARANTINE123", sha);
    }
}
