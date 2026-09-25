namespace QuantForge.Core;

public sealed record ProductUiStrategyAuditState(
    string StrategyId,
    StrategyAdmissionState AdmissionState,
    string QuarantineFingerprint,
    string SanitizedFingerprint,
    IReadOnlyList<StrategyFeature> EnabledFeatures,
    bool DeclaresOrderSubmission,
    bool DeclaresApplicationSettingsMutation,
    bool DeclaresLiveAccountRequirement,
    bool RequiresSecurityReview,
    bool IsResearchAdmitted,
    bool UiCanSubmitOrders,
    bool UiCanChangeApplicationSettings,
    string? Warning);

public static class ProductUiStrategyAuditPresenter
{
    public static ProductUiStrategyAuditState Create(StrategyAdmissionEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        if (string.IsNullOrWhiteSpace(envelope.Manifest.StrategyId))
            throw new InvalidOperationException("Strategy audit UI requires strategy identity.");

        if (string.IsNullOrWhiteSpace(envelope.Manifest.SourceFingerprint))
            throw new InvalidOperationException("Strategy audit UI requires source identity.");

        if (envelope.Selection.EnabledFeatures is null)
            throw new InvalidOperationException("Strategy audit UI requires an explicit feature selection.");

        var declaredOrder = envelope.Manifest.CanSubmitOrders ||
                            envelope.Selection.EnabledFeatures.Contains(StrategyFeature.OrderSubmission);
        var declaredSettings = envelope.Manifest.CanChangeApplicationSettings ||
                               envelope.Selection.EnabledFeatures.Contains(StrategyFeature.ApplicationSettingsMutation);
        var declaredLive = envelope.Manifest.RequiresLiveAccount;
        var requiresSecurityReview = envelope.Manifest.UsesProcessExecution ||
                                     envelope.Manifest.UsesNativeLibrary ||
                                     envelope.Selection.EnabledFeatures.Contains(StrategyFeature.ProcessExecution) ||
                                     envelope.Selection.EnabledFeatures.Contains(StrategyFeature.NativeLibraryAccess);

        var admitted = envelope.State == StrategyAdmissionState.Admitted;
        if (admitted)
            StrategyAdmissionPipeline.RequireAdmitted(envelope);

        var warning = admitted
            ? null
            : requiresSecurityReview || declaredOrder || declaredSettings || declaredLive
                ? "Strategy remains outside research admission and contains capabilities requiring removal or explicit security review."
                : "Strategy has not completed sanitized research admission."
;

        return new ProductUiStrategyAuditState(
            envelope.Manifest.StrategyId,
            envelope.State,
            envelope.QuarantineFingerprint,
            envelope.SanitizedFingerprint,
            envelope.Selection.EnabledFeatures.ToArray(),
            declaredOrder,
            declaredSettings,
            declaredLive,
            requiresSecurityReview,
            admitted,
            UiCanSubmitOrders: false,
            UiCanChangeApplicationSettings: false,
            warning);
    }
}
