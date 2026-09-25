using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public class SimulationIsolationTests
{
    [Fact]
    public void Invalid_ohlc_is_rejected()
    {
        var market = new MarketEvent(
            1,
            DateTimeOffset.UtcNow,
            100m, 99m, 98m, 98.5m, 10m);

        Assert.Throws<InvalidOperationException>(market.Validate);
    }

    [Fact]
    public void Fill_cannot_precede_signal()
    {
        var intent = new SimulationIntent(
            "s1", "batch|s1|account", SimulationSide.Buy,
            SimulationIntentType.Market, 1m,
            new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 1, 1, 11, 59, 0, TimeSpan.Zero));

        Assert.Throws<InvalidOperationException>(intent.ValidateResearchOnly);
    }

    [Fact]
    public void Multi_strategy_batch_rejects_duplicate_ids()
    {
        var first = new ResearchStrategyInstance(
            "s1", "sha1", new LedgerNamespace("b", "s1", "a1"));
        var second = new ResearchStrategyInstance(
            "s1", "sha2", new LedgerNamespace("b", "s2", "a2"));

        Assert.Throws<InvalidOperationException>(
            () => ResearchBatchRules.RequireIndependentStrategies(new[] { first, second }));
    }

    [Fact]
    public void Multi_strategy_batch_accepts_isolated_instances()
    {
        var first = new ResearchStrategyInstance(
            "s1", "sha1", new LedgerNamespace("b", "s1", "a1"));
        var second = new ResearchStrategyInstance(
            "s2", "sha2", new LedgerNamespace("b", "s2", "a2"));

        ResearchBatchRules.RequireIndependentStrategies(new[] { first, second });
    }
}