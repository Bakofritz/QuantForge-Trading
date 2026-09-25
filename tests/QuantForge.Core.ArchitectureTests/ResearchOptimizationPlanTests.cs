using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public class ResearchOptimizationPlanTests
{
    [Fact]
    public void Optimization_plan_accepts_parameter_variants_with_independent_ledgers()
    {
        var first = Request("params-a", "job-a", "batch|s1|account-a");
        var second = Request("params-b", "job-b", "batch|s1|account-b");

        var plan = ResearchOptimizationPlanRules.Create(new[] { first, second });

        Assert.Equal(2, plan.Variants.Count);
        Assert.NotEqual(
            plan.Variants[0].Job.Identity.ParameterSetFingerprint,
            plan.Variants[1].Job.Identity.ParameterSetFingerprint);
    }

    [Fact]
    public void Optimization_plan_rejects_duplicate_parameter_partition_variant()
    {
        var first = Request("params-a", "job-a", "batch|s1|account-a");
        var duplicate = Request("params-a", "job-b", "batch|s1|account-b");

        Assert.Throws<InvalidOperationException>(() =>
            ResearchOptimizationPlanRules.Create(new[] { first, duplicate }));
    }

    [Fact]
    public void Optimization_plan_rejects_shared_ledger_namespace()
    {
        var first = Request("params-a", "job-a", "batch|s1|account-a");
        var second = Request("params-b", "job-b", "batch|s1|account-a");

        Assert.Throws<InvalidOperationException>(() =>
            ResearchOptimizationPlanRules.Create(new[] { first, second }));
    }

    [Fact]
    public void Optimization_plan_variants_execute_through_reliability_gated_workflow()
    {
        var first = Request("params-a", "job-a", "batch|s1|account-a");
        var second = Request("params-b", "job-b", "batch|s1|account-b");
        var plan = ResearchOptimizationPlanRules.Create(new[] { first, second });
        var reliability = new DataReliabilityAssessment("data-sha", 97m, true, 0, 0, null);

        var result = ResearchWorkflowCoordinator.Run(new ResearchWorkflowRequest(
            new ResearchBatchRequest(ResearchBatchMode.ReadOnlyOptimization, plan.Variants),
            new[] { reliability }));

        Assert.Equal(2, result.Reports.Count);
        Assert.All(result.Reports, report => Assert.Equal(ResearchResultStatus.Complete, report.Status));
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
