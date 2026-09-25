using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ProductUiOptimizationPresenterTests
{
    [Fact]
    public void OptimizationUi_PreservesVariantIdentityAndLedgerIsolation()
    {
        var plan = ResearchOptimizationPlanRules.Create(new[]
        {
            Request("params-b", "job-b", "batch|s1|account-b"),
            Request("params-a", "job-a", "batch|s1|account-a")
        });

        var state = ProductUiOptimizationPresenter.Create(plan);

        Assert.Equal(2, state.VariantCount);
        Assert.False(state.LiveAccountEnabled);
        Assert.False(state.CanSubmitOrders);
        Assert.False(state.CanChangeApplicationSettings);
        Assert.Equal(new[] { "job-a", "job-b" }, state.Variants.Select(x => x.JobFingerprint));
        Assert.Equal(2, state.Variants.Select(x => x.LedgerNamespace).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void OptimizationUi_RejectsUnsafeLiveVariant()
    {
        var request = Request("params-a", "job-a", "batch|s1|account-a");
        var unsafeJob = request.Job with { Authority = AuthorityDomain.LiveAccount };
        var unsafePlan = new ResearchOptimizationPlan(new[] { request with { Job = unsafeJob } });

        Assert.Throws<InvalidOperationException>(() =>
            ProductUiOptimizationPresenter.Create(unsafePlan));
    }

    private static ResearchRunRequest Request(
        string parameterFingerprint,
        string jobFingerprint,
        string ledgerNamespace)
    {
        var time = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var data = new DataAdmission(
            "dataset", "data-sha", "TEST", "1m", time.AddHours(-1), time.AddHours(1), true, true);
        var strategy = new StrategyCapabilityManifest(
            "s1", "strategy-1", false, false, false, false, false, false, false);
        var identity = new ResearchJobIdentity(
            "data-sha", "strategy-1", "exec-sha", parameterFingerprint, "partition", jobFingerprint);
        var job = new ResearchJobSpec(
            AuthorityDomain.SimulatedAccount, data, strategy, identity, ExecutionTimingPolicy.NextBarOpen, false);
        var intent = new SimulationIntent(
            "s1", ledgerNamespace, SimulationSide.Buy, SimulationIntentType.Market, 1m, time, time.AddMinutes(1));
        var market = new MarketEvent(1, time.AddMinutes(1), 100m, 100m, 100m, 100m, 10m);
        var admission = new StrategyAdmissionEnvelope(
            StrategyAdmissionState.Admitted,
            strategy,
            new StrategyFeatureSelection(new[] { StrategyFeature.SignalGeneration }),
            "quarantine-strategy-1",
            "strategy-1");

        return new ResearchRunRequest(job, new[] { intent }, new[] { market }, 1000m, 0m, 0m, admission);
    }
}
