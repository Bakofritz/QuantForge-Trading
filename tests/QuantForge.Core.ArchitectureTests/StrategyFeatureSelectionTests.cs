using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public class StrategyFeatureSelectionTests
{
    [Fact]
    public void Research_selection_rejects_order_submission()
    {
        var selection = new StrategyFeatureSelection(
            new[] { StrategyFeature.SignalGeneration, StrategyFeature.OrderSubmission });

        Assert.Throws<InvalidOperationException>(
            () => StrategyFeatureSelectionRules.RequireResearchSafe(selection));
    }

    [Fact]
    public void Research_selection_accepts_safe_features()
    {
        var selection = new StrategyFeatureSelection(
            new[] { StrategyFeature.SignalGeneration, StrategyFeature.StopsAndTargets });

        StrategyFeatureSelectionRules.RequireResearchSafe(selection);
    }
}
