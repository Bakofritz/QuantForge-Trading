using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class SessionCoverageTests
{
    private static SessionCoveragePolicy Policy(bool authoritative = true) => new(
        "fixture-session",
        "1",
        "calendar-sha256",
        new[]
        {
            new SessionInterval(
                new DateTimeOffset(2026, 1, 5, 14, 30, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 1, 5, 14, 33, 0, TimeSpan.Zero))
        },
        authoritative);

    private static MarketEvent[] Bars(params int[] minutes) => minutes
        .Select(minute => new MarketEvent(
            minute,
            new DateTimeOffset(2026, 1, 5, 14, minute, 0, TimeSpan.Zero),
            100m, 101m, 99m, 100m, 1m))
        .ToArray();

    [Fact]
    public void Complete_authoritative_session_is_research_admissible()
    {
        var report = SessionCoverageRules.Analyze("dataset-sha", Bars(30, 31, 32), Policy());

        Assert.True(report.Complete);
        Assert.True(report.ResearchAdmissible);
        Assert.Equal(3, report.ExpectedMinuteCount);
        Assert.Equal(3, report.ObservedInSessionMinuteCount);
    }

    [Fact]
    public void Missing_session_minute_blocks_admission()
    {
        var report = SessionCoverageRules.Analyze("dataset-sha", Bars(30, 32), Policy());

        Assert.False(report.Complete);
        Assert.False(report.ResearchAdmissible);
        Assert.Single(report.MissingMinutes);
    }

    [Fact]
    public void Outside_session_observation_blocks_admission()
    {
        var report = SessionCoverageRules.Analyze("dataset-sha", Bars(30, 31, 32, 33), Policy());

        Assert.False(report.Complete);
        Assert.False(report.ResearchAdmissible);
        Assert.Single(report.OutsideSessionMinutes);
    }

    [Fact]
    public void Non_authoritative_policy_never_admits_even_when_complete()
    {
        var report = SessionCoverageRules.Analyze("dataset-sha", Bars(30, 31, 32), Policy(false));

        Assert.True(report.Complete);
        Assert.False(report.ResearchAdmissible);
        Assert.Contains("not authoritative", report.Limitation);
    }

    [Fact]
    public void Overlapping_sessions_are_rejected()
    {
        var policy = Policy() with
        {
            Sessions = new[]
            {
                new SessionInterval(
                    new DateTimeOffset(2026, 1, 5, 14, 30, 0, TimeSpan.Zero),
                    new DateTimeOffset(2026, 1, 5, 14, 32, 0, TimeSpan.Zero)),
                new SessionInterval(
                    new DateTimeOffset(2026, 1, 5, 14, 31, 0, TimeSpan.Zero),
                    new DateTimeOffset(2026, 1, 5, 14, 33, 0, TimeSpan.Zero))
            }
        };

        Assert.Throws<InvalidOperationException>(() =>
            SessionCoverageRules.Analyze("dataset-sha", Bars(30, 31, 32), policy));
    }
}

public sealed class SessionCoverageWorkflowTests
{
    [Fact]
    public void Supplied_session_coverage_is_enforced_by_research_workflow()
    {
        var time = new DateTimeOffset(2026, 1, 5, 14, 30, 0, TimeSpan.Zero);
        var data = new DataAdmission("dataset", "data-sha", "TEST", "1m", time, time.AddMinutes(3), true, true);
        var strategy = new StrategyCapabilityManifest("s1", "strategy-sha", false, false, false, false, false, false, false);
        var job = new ResearchJobSpec(
            AuthorityDomain.SimulatedAccount,
            data,
            strategy,
            new ResearchJobIdentity("data-sha", "strategy-sha", "exec", "params", "partition", "job-1"),
            ExecutionTimingPolicy.NextBarOpen,
            false);
        var intent = new SimulationIntent("s1", "batch|s1|account", SimulationSide.Buy, SimulationIntentType.Market, 1m, time, time.AddMinutes(1));
        var market = new MarketEvent(1, time.AddMinutes(1), 100m, 100m, 100m, 100m, 10m);
        var run = new ResearchRunRequest(job, new[] { intent }, new[] { market }, 1000m, 0m, 0m);
        var reliability = new DataReliabilityAssessment("data-sha", 99m, true, 0, 0, null);
        var incomplete = SessionCoverageRules.Analyze(
            "data-sha",
            new[] { new MarketEvent(30, time, 100m, 101m, 99m, 100m, 1m), new MarketEvent(32, time.AddMinutes(2), 100m, 101m, 99m, 100m, 1m) },
            new SessionCoveragePolicy("fixture", "1", "calendar", new[] { new SessionInterval(time, time.AddMinutes(3)) }, true));

        var result = ResearchWorkflowCoordinator.Run(new ResearchWorkflowRequest(
            new ResearchBatchRequest(ResearchBatchMode.ReadOnlyResearch, new[] { run }),
            new[] { reliability },
            new[] { incomplete }));

        Assert.Equal(ResearchResultStatus.DataBlocked, Assert.Single(result.Reports).Status);
    }
}
