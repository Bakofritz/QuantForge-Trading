using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public class SimulatedAccountTests
{
    [Fact]
    public void Buy_then_sell_records_realized_pnl_and_equity()
    {
        var account = new SimulatedAccount("batch|s1|account", 1000m);
        var buy = new SimulationFill("s1", "batch|s1|account", SimulationSide.Buy,
            new DateTimeOffset(2026, 1, 2, 10, 0, 0, TimeSpan.Zero),
            100m, 2m, 2m, 0.5m);
        account.ApplyFill(buy);

        var sell = new SimulationFill("s1", "batch|s1|account", SimulationSide.Sell,
            new DateTimeOffset(2026, 1, 2, 11, 0, 0, TimeSpan.Zero),
            110m, 1m, 1m, 0.2m);
        var snapshot = account.ApplyFill(sell);

        Assert.Equal(1m, snapshot.Position.Quantity);
        Assert.Equal(8m, snapshot.RealizedPnl);
        Assert.Equal(10m, snapshot.UnrealizedPnl);
        Assert.Equal(1017m, snapshot.Equity);
    }

    [Fact]
    public void Mismatched_ledger_namespace_is_rejected()
    {
        var account = new SimulatedAccount("batch|s1|account", 1000m);
        var fill = new SimulationFill("s1", "other", SimulationSide.Buy, DateTimeOffset.UtcNow, 100m, 1m, 0m, 0m);

        Assert.Throws<InvalidOperationException>(() => account.ApplyFill(fill));
    }
}
