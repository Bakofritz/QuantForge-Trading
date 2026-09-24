namespace QuantForge.Core;

public readonly record struct StrategyCapabilityManifest(
    string StrategyId,
    string SourceFingerprint,
    bool CanSubmitOrders,
    bool CanChangeApplicationSettings,
    bool UsesNetwork,
    bool UsesFilesystem,
    bool UsesProcessExecution,
    bool UsesNativeLibrary,
    bool RequiresLiveAccount);

public static class StrategyAdmissionRules
{
    public static void RequireResearchSafe(StrategyCapabilityManifest manifest)
    {
        if (string.IsNullOrWhiteSpace(manifest.StrategyId))
            throw new InvalidOperationException("Strategy identity is required.");

        if (string.IsNullOrWhiteSpace(manifest.SourceFingerprint))
            throw new InvalidOperationException("Strategy source fingerprint is required.");

        if (manifest.CanSubmitOrders ||
            manifest.CanChangeApplicationSettings ||
            manifest.RequiresLiveAccount)
            throw new InvalidOperationException(
                "Strategy is not eligible for research-only admission.");

        if (manifest.UsesProcessExecution ||
            manifest.UsesNativeLibrary)
            throw new InvalidOperationException(
                "Unreviewed process or native-library capability is not eligible for automatic research admission.");
    }
}
