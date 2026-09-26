using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class WalkForwardPerformanceSummaryTests
{
    [Fact]
    public void Summary_uses_only_completed_out_of_sample_evaluations()
    {
        var result = WalkForwardResearchRunner.Run(WalkForwardResearchPlanRules.Create(new[]
        {
            WalkForwardResearchPlanTests.FixtureSegment()
        }));

        var summary = WalkForwardPerformanceSummaryRules.Create(result);

        Assert.Equal(result.ResultFingerprint, summary.WalkForwardResultFingerprint);
        Assert.Equal(1, summary.SegmentCount);
        var evaluation = Assert.Single(result.Segments).Evaluation!.Value;
        Assert.Equal(evaluation.Account!.Value.Equity, summary.MeanOutOfSampleFinalEquity);
        Assert.Equal(evaluation.Account.Value.RealizedPnl, summary.TotalOutOfSampleRealizedPnl);
        Assert.False(string.IsNullOrWhiteSpace(summary.SummaryFingerprint));
    }

    [Fact]
    public void Summary_rejects_blocked_walk_forward_result()
    {
        var result = WalkForwardResearchRunner.Run(WalkForwardResearchPlanRules.Create(new[]
        {
            WalkForwardResearchPlanTests.FixtureSegment()
        }));
        var blocked = result with { Complete = false, BlockReason = "blocked after validation" };
        Assert.Throws<InvalidOperationException>(() => WalkForwardPerformanceSummaryRules.Create(blocked));
    }
}
