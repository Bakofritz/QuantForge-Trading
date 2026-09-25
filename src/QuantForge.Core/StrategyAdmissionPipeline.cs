namespace QuantForge.Core;

public enum StrategyAdmissionState
{
    Quarantined,
    Sanitized,
    Admitted
}

public sealed record StrategyAdmissionEnvelope(
    StrategyAdmissionState State,
    StrategyCapabilityManifest Manifest,
    StrategyFeatureSelection Selection,
    string QuarantineFingerprint,
    string SanitizedFingerprint);

public static class StrategyAdmissionPipeline
{
    public static StrategyCapabilityManifest RequireAdmitted(StrategyAdmissionEnvelope envelope)
    {
        if (envelope.State != StrategyAdmissionState.Admitted)
            throw new InvalidOperationException("Strategy must complete quarantine, scrubbing, feature selection, and sanitized admission before research execution.");

        if (string.IsNullOrWhiteSpace(envelope.QuarantineFingerprint) ||
            string.IsNullOrWhiteSpace(envelope.SanitizedFingerprint))
            throw new InvalidOperationException("Strategy admission provenance is incomplete.");

        StrategyFeatureSelectionRules.RequireResearchSafe(envelope.Selection);
        StrategyAdmissionRules.RequireResearchSafe(envelope.Manifest);

        if (!string.Equals(envelope.Manifest.SourceFingerprint, envelope.SanitizedFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Admitted strategy fingerprint must match the sanitized strategy fingerprint.");

        return envelope.Manifest;
    }
}
