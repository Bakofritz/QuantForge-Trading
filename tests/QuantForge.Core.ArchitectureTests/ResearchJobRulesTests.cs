using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public class ResearchJobRulesTests
{
    private static ResearchJobSpec ValidJob(ExecutionTimingPolicy timing = ExecutionTimingPolicy.NextBarOpen) =>
        new(
            AuthorityDomain.ReadOnlyResearch,
            new DataAdmission(
                "MES-NT8-MINUTE",
                "dataset-sha256",
                "MES",
                "1m",
                new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                true,
                true),
            new StrategyCapabilityManifest(
                "example",
                "strategy-sha256",
                false,
                false,
                false,
                false,
                false,
                false,
                false),
            new ResearchJobIdentity(
                "dataset-sha256",
                "strategy-sha256",
                "timing-sha256",
                "parameters-sha256",
                "walk-forward-01",
                "job-sha256"),
            timing,
            false);

    [Fact]
    public void Valid_research_job_is_admitted()
    {
        ResearchJobRules.RequireRunnable(ValidJob());
    }

    [Fact]
    public void Same_bar_fill_requires_close_auction()
    {
        Assert.Throws<InvalidOperationException>(
            () => ResearchJobRules.RequireRunnable(
                ValidJob(ExecutionTimingPolicy.NextBarOpen) with { SignalAndFillShareBar = true }));
    }
}
