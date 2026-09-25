namespace QuantForge.Core.ArchitectureTests;

public sealed class ProductUiStrategyAuditPresenterTests
{
    [Fact]
    public void QuarantinedUnsafeStrategy_IsVisibleButNeverGrantedUiAuthority()
    {
        var envelope = new StrategyAdmissionEnvelope(
            StrategyAdmissionState.Quarantined,
            new StrategyCapabilityManifest(
                "unsafe-strategy",
                "original-fingerprint",
                CanSubmitOrders: true,
                CanChangeApplicationSettings: true,
                UsesNetwork: false,
                UsesFilesystem: false,
                UsesProcessExecution: true,
                UsesNativeLibrary: false,
                RequiresLiveAccount: true),
            new StrategyFeatureSelection(new[]
            {
                StrategyFeature.SignalGeneration,
                StrategyFeature.OrderSubmission,
                StrategyFeature.ProcessExecution
            }),
            "quarantine-fingerprint",
            string.Empty);

        var state = ProductUiStrategyAuditPresenter.Create(envelope);

        Assert.Equal(StrategyAdmissionState.Quarantined, state.AdmissionState);
        Assert.False(state.IsResearchAdmitted);
        Assert.False(state.UiCanSubmitOrders);
        Assert.False(state.UiCanChangeApplicationSettings);
        Assert.True(state.DeclaresOrderSubmission);
        Assert.True(state.DeclaresApplicationSettingsMutation);
        Assert.True(state.DeclaresLiveAccountRequirement);
        Assert.True(state.RequiresSecurityReview);
        Assert.False(string.IsNullOrWhiteSpace(state.Warning));
    }

    [Fact]
    public void AdmittedUnsafeStrategy_IsRejectedInsteadOfDisplayedAsAdmitted()
    {
        var envelope = new StrategyAdmissionEnvelope(
            StrategyAdmissionState.Admitted,
            new StrategyCapabilityManifest(
                "unsafe-strategy",
                "sanitized-fingerprint",
                CanSubmitOrders: true,
                CanChangeApplicationSettings: false,
                UsesNetwork: false,
                UsesFilesystem: false,
                UsesProcessExecution: false,
                UsesNativeLibrary: false,
                RequiresLiveAccount: false),
            new StrategyFeatureSelection(new[] { StrategyFeature.SignalGeneration }),
            "quarantine-fingerprint",
            "sanitized-fingerprint");

        Assert.Throws<InvalidOperationException>(() =>
            ProductUiStrategyAuditPresenter.Create(envelope));
    }

    [Fact]
    public void AdmittedResearchSafeStrategy_IsPresentedAsResearchAdmittedOnly()
    {
        var envelope = new StrategyAdmissionEnvelope(
            StrategyAdmissionState.Admitted,
            new StrategyCapabilityManifest(
                "safe-strategy",
                "sanitized-fingerprint",
                CanSubmitOrders: false,
                CanChangeApplicationSettings: false,
                UsesNetwork: false,
                UsesFilesystem: false,
                UsesProcessExecution: false,
                UsesNativeLibrary: false,
                RequiresLiveAccount: false),
            new StrategyFeatureSelection(new[]
            {
                StrategyFeature.SignalGeneration,
                StrategyFeature.Visualization
            }),
            "quarantine-fingerprint",
            "sanitized-fingerprint");

        var state = ProductUiStrategyAuditPresenter.Create(envelope);

        Assert.True(state.IsResearchAdmitted);
        Assert.False(state.UiCanSubmitOrders);
        Assert.False(state.UiCanChangeApplicationSettings);
        Assert.Null(state.Warning);
    }
}
