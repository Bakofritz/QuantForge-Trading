namespace QuantForge.Core;

public enum StrategyFeature
{
    SignalGeneration,
    PositionSizing,
    StopsAndTargets,
    Alerts,
    Visualization,
    ExternalNetwork,
    FilesystemAccess,
    ProcessExecution,
    NativeLibraryAccess,
    ApplicationSettingsMutation,
    OrderSubmission
}

public readonly record struct StrategyFeatureSelection(
    StrategyFeature[] EnabledFeatures);

public static class StrategyFeatureSelectionRules
{
    public static void RequireResearchSafe(StrategyFeatureSelection selection)
    {
        if (selection.EnabledFeatures.Contains(StrategyFeature.OrderSubmission))
            throw new InvalidOperationException(
                "Order submission is not a research-safe selectable feature.");

        if (selection.EnabledFeatures.Contains(StrategyFeature.ApplicationSettingsMutation))
            throw new InvalidOperationException(
                "Application-setting mutation is not a research-safe selectable feature.");

        if (selection.EnabledFeatures.Contains(StrategyFeature.ProcessExecution) ||
            selection.EnabledFeatures.Contains(StrategyFeature.NativeLibraryAccess))
            throw new InvalidOperationException(
                "Process and native-library execution require explicit security review before admission.");
    }
}
