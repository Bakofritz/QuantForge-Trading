using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ProductUiWalkForwardPresenterTests
{
    [Fact]
    public void Presenter_exposes_verified_out_of_sample_results_without_trading_authority()
    {
        var result = WalkForwardResearchRunner.Run(WalkForwardResearchPlanRules.Create(new[]
        {
            WalkForwardResearchPlanTests.FixtureSegment()
        }));

        var state = ProductUiWalkForwardPresenter.Create(result);

        Assert.True(state.Complete);
        Assert.False(state.LiveAccountEnabled);
        Assert.False(state.CanSubmitOrders);
        Assert.False(state.CanChangeApplicationSettings);
        var segment = Assert.Single(state.Segments);
        Assert.Equal("job-a", segment.SelectedTrainingJobId);
        Assert.Equal("job-eval", segment.EvaluationJobId);
        Assert.Equal(ResearchResultStatus.Complete, segment.EvaluationStatus);
        Assert.NotNull(segment.FinalEquity);
    }

    [Fact]
    public void Presenter_rejects_tampered_result_fingerprint()
    {
        var result = WalkForwardResearchRunner.Run(WalkForwardResearchPlanRules.Create(new[]
        {
            WalkForwardResearchPlanTests.FixtureSegment()
        }));

        Assert.Throws<InvalidOperationException>(() =>
            ProductUiWalkForwardPresenter.Create(result with { ResultFingerprint = "tampered" }));
    }
}
