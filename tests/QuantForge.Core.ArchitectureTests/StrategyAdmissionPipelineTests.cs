using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public class StrategyAdmissionPipelineTests
{
    [Fact]
    public void Admission_requires_completed_pipeline_state()
    {
        var envelope = Envelope(StrategyAdmissionState.Quarantined);
        Assert.Throws<InvalidOperationException>(() => StrategyAdmissionPipeline.RequireAdmitted(envelope));
    }

    [Fact]
    public void Admission_rejects_unsafe_selected_features()
    {
        var envelope = Envelope(
            StrategyAdmissionState.Admitted,
            new StrategyFeatureSelection(new[] { StrategyFeature.OrderSubmission }));

        Assert.Throws<InvalidOperationException>(() => StrategyAdmissionPipeline.RequireAdmitted(envelope));
    }

    [Fact]
    public void Admission_binds_manifest_to_sanitized_fingerprint()
    {
        var envelope = Envelope(StrategyAdmissionState.Admitted) with
        {
            SanitizedFingerprint = "different-sha"
        };

        Assert.Throws<InvalidOperationException>(() => StrategyAdmissionPipeline.RequireAdmitted(envelope));
    }

    [Fact]
    public void Admission_accepts_research_safe_sanitized_strategy()
    {
        var envelope = Envelope(StrategyAdmissionState.Admitted);
        var manifest = StrategyAdmissionPipeline.RequireAdmitted(envelope);

        Assert.Equal("s1", manifest.StrategyId);
        Assert.Equal("sanitized-sha", manifest.SourceFingerprint);
    }

    private static StrategyAdmissionEnvelope Envelope(
        StrategyAdmissionState state,
        StrategyFeatureSelection? selection = null)
        => new(
            state,
            new StrategyCapabilityManifest(
                "s1", "sanitized-sha",
                false, false, false, false, false, false, false),
            selection ?? new StrategyFeatureSelection(
                new[] { StrategyFeature.SignalGeneration, StrategyFeature.PositionSizing }),
            "quarantine-sha",
            "sanitized-sha");
}
