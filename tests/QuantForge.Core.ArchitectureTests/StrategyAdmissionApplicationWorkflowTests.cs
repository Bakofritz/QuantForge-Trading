using QuantForge.Core;
using Xunit;

namespace QuantForge.Core.ArchitectureTests;

public sealed class StrategyAdmissionApplicationWorkflowTests
{
    [Fact]
    public void Persist_round_trips_only_admitted_strategy_artifact()
    {
        var root = Path.Combine(Path.GetTempPath(), "qf-strategy-app-" + Guid.NewGuid().ToString("N"));
        try
        {
            var workflow = new StrategyAdmissionApplicationWorkflow(new StrategyAdmissionArtifactFileStore(root));
            var result = workflow.Persist(Envelope());
            Assert.Equal(result.Artifact, workflow.Recover(result.Artifact.ArtifactFingerprint));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void Persist_does_not_promote_sanitized_strategy()
    {
        var root = Path.Combine(Path.GetTempPath(), "qf-strategy-app-" + Guid.NewGuid().ToString("N"));
        try
        {
            var workflow = new StrategyAdmissionApplicationWorkflow(new StrategyAdmissionArtifactFileStore(root));
            Assert.Throws<InvalidOperationException>(() => workflow.Persist(Envelope() with { State = StrategyAdmissionState.Sanitized }));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    private static StrategyAdmissionEnvelope Envelope() => new(
        StrategyAdmissionState.Admitted,
        new StrategyCapabilityManifest("strategy-1", "ABCDEF0123456789", false, false, false, false, false, false, false),
        new StrategyFeatureSelection(new[] { StrategyFeature.SignalGeneration }),
        "QUARANTINE123", "ABCDEF0123456789");
}
