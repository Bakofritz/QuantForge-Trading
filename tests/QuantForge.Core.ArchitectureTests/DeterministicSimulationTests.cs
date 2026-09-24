using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public class DeterministicSimulationTests
{
    [Fact]
    public void Next_eligible_market_event_produces_deterministic_fill()
    {
        var intent = new SimulationIntent(
            "s1", "batch|s1|account", SimulationSide.Buy,
            SimulationIntentType.Market, 2m,
            new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 1, 1, 12, 1, 0, TimeSpan.Zero));

        var market = new MarketEvent(
            1,
            new DateTimeOffset(2026, 1, 1, 12, 1, 0, TimeSpan.Zero),
            100m, 101m, 99m, 100.5m, 1000m);

        var fill = DeterministicFillModel.FillAtNextEligibleEvent(
            intent, market, 0.50m, 0.25m);

        Assert.Equal(100.25m, fill.Price);
        Assert.Equal(1.00m, fill.Commission);
        Assert.Equal(0.50m, fill.Slippage);
    }

    [Fact]
    public void Evidence_chain_fingerprint_changes_when_payload_changes()
    {
        var root = new EvidenceRecord(0, string.Empty, "root", "root", "root");

        var first = EvidenceChain.Append(root, 1, "event", "A");
        var second = EvidenceChain.Append(root, 1, "event", "B");

        Assert.NotEqual(first.Fingerprint, second.Fingerprint);
        Assert.Equal(root.Fingerprint, first.PreviousFingerprint);
    }
}
