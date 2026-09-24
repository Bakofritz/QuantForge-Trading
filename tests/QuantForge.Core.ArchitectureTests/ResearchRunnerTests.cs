using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public class ResearchRunnerTests
{
    [Fact]
    public void Runner_uses_first_eligible_event_and_produces_deterministic_account()
    {
        var request = CreateRequest(
            new[]
            {
                new SimulationIntent(
                    "s1", "batch|s1|account", SimulationSide.Buy,
                    SimulationIntentType.Market, 2m,
                    At(10, 0), At(10, 1))
            },
            new[]
            {
                Event(1, 10, 0, 99m),
                Event(2, 10, 1, 100m),
                Event(3, 10, 2, 105m)
            });

        var first = DeterministicResearchRunner.Run(request);
        var second = DeterministicResearchRunner.Run(request);

        Assert.Equal(ResearchResultStatus.Complete, first.Status);
        Assert.Equal(first, second);
        Assert.NotNull(first.Account);
        Assert.Equal(2m, first.Account.Value.Position.Quantity);
        Assert.Equal(100m, first.Account.Value.Position.AveragePrice);
        Assert.Equal(1010m, first.Account.Value.Equity);
    }

    [Fact]
    public void Runner_blocks_without_eligible_data_and_creates_no_performance_state()
    {
        var request = CreateRequest(
            new[]
            {
                new SimulationIntent(
                    "s1", "batch|s1|account", SimulationSide.Buy,
                    SimulationIntentType.Market, 1m,
                    At(10, 0), At(11, 0))
            },
            new[] { Event(1, 10, 0, 100m) });

        var report = DeterministicResearchRunner.Run(request);

        Assert.Equal(ResearchResultStatus.DataBlocked, report.Status);
        Assert.Null(report.Account);
        Assert.Contains("No eligible market event", report.BlockReason);
    }

    [Fact]
    public void Runner_rejects_invalid_admission_without_creating_performance_state()
    {
        var request = CreateRequest(
            new[]
            {
                new SimulationIntent(
                    "s1", "batch|s1|account", SimulationSide.Buy,
                    SimulationIntentType.Market, 1m,
                    At(10, 0), At(10, 1))
            },
            new[] { Event(1, 10, 1, 100m) },
            admitted: false);

        var report = DeterministicResearchRunner.Run(request);

        Assert.Equal(ResearchResultStatus.Invalid, report.Status);
        Assert.Null(report.Account);
        Assert.Contains("Data admission", report.BlockReason);
    }

    [Fact]
    public void Batch_rejects_shared_strategy_identity()
    {
        var first = CreateRequest(
            new[]
            {
                new SimulationIntent(
                    "s1", "batch|s1|account", SimulationSide.Buy,
                    SimulationIntentType.Market, 1m,
                    At(10, 0), At(10, 1))
            },
            new[] { Event(1, 10, 1, 100m) });

        var second = CreateRequest(
            new[]
            {
                new SimulationIntent(
                    "s1", "batch|s2|account", SimulationSide.Buy,
                    SimulationIntentType.Market, 1m,
                    At(10, 0), At(10, 1))
            },
            new[] { Event(1, 10, 1, 100m) });

        Assert.Throws<InvalidOperationException>(
            () => DeterministicResearchRunner.RunBatch(new[] { first, second }));
    }

    private static ResearchRunRequest CreateRequest(
        IReadOnlyList<SimulationIntent> intents,
        IReadOnlyList<MarketEvent> events,
        bool admitted = true)
    {
        var data = new DataAdmission(
            "dataset-1", "data-sha", "TEST", "1m",
            At(9, 0), At(12, 0),
            admitted, admitted);

        var strategy = new StrategyCapabilityManifest(
            "s1", "strategy-sha",
            false, false, false, false, false, false, false);

        var identity = new ResearchJobIdentity(
            "data-sha", "strategy-sha", "exec-sha",
            "params-sha", "2026-01-01", "job-1");

        var job = new ResearchJobSpec(
            AuthorityDomain.SimulatedAccount,
            data,
            strategy,
            identity,
            ExecutionTimingPolicy.NextBarOpen,
            false);

        return new ResearchRunRequest(
            job, intents, events, 1000m, 0m, 0m);
    }

    private static MarketEvent Event(
        long sequence, int hour, int minute, decimal open)
        => new(
            sequence,
            At(hour, minute),
            open, open, open, open, 100m);

    private static DateTimeOffset At(int hour, int minute)
        => new(2026, 1, 1, hour, minute, 0, TimeSpan.Zero);
}
