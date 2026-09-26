using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ProductUiOptimizationResultPresenterTests
{
    [Fact]
    public void ResultUi_VerifiesSelectionAndExposesOnlyReadOnlyMetrics()
    {
        var reports = new[]
        {
            Complete("job-b", 1200m, 50m),
            Complete("job-a", 1200m, 40m)
        };
        var evaluation = new ResearchOptimizationResult(reports, true, null);
        var selection = ResearchOptimizationSelectionRules.Select(evaluation, ResearchOptimizationObjective.FinalEquity);
        var result = new ResearchOptimizationApplicationResult(evaluation, selection, "result-sha");

        var state = ProductUiOptimizationResultPresenter.Create(result);

        Assert.True(state.Complete);
        Assert.Equal("job-a", state.SelectedJobId);
        Assert.Equal(ResearchOptimizationObjective.FinalEquity, state.Objective);
        Assert.False(state.LiveAccountEnabled);
        Assert.False(state.CanSubmitOrders);
        Assert.False(state.CanChangeApplicationSettings);
        Assert.Equal(new[] { "job-a", "job-b" }, state.Variants.Select(x => x.JobId));
        Assert.Single(state.Variants.Where(x => x.Selected));
    }

    [Fact]
    public void ResultUi_PreservesBlockedOutcomeWithoutSelectionOrPerformance()
    {
        var report = new ResearchReport(
            "job-blocked", ResearchResultStatus.DataBlocked, "data", "strategy", "exec", "params", "partition",
            "coverage blocked", null, null);
        var result = new ResearchOptimizationApplicationResult(
            new ResearchOptimizationResult(new[] { report }, false, "Optimization blocked by variant job-blocked."),
            null,
            "blocked-sha");

        var state = ProductUiOptimizationResultPresenter.Create(result);

        Assert.False(state.Complete);
        Assert.Null(state.SelectedJobId);
        Assert.Null(state.Objective);
        Assert.Null(state.Variants[0].FinalEquity);
        Assert.Null(state.Variants[0].RealizedPnl);
        Assert.Equal("coverage blocked", state.Variants[0].Message);
    }

    private static ResearchReport Complete(string jobId, decimal equity, decimal realized) =>
        new(
            jobId,
            ResearchResultStatus.Complete,
            "data",
            "strategy",
            "exec",
            "params-" + jobId,
            "partition",
            null,
            new AccountSnapshot(
                "ledger-" + jobId,
                1000m,
                equity,
                new SimulatedPosition(0m, 0m),
                0m,
                realized,
                0m,
                equity),
            new EvidenceRecord(1, "previous", "completed", "payload", "evidence-" + jobId));
}
