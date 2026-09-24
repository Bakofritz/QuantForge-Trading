namespace QuantForge.Core;

public readonly record struct SimulationFill(
    string StrategyId,
    string LedgerNamespace,
    DateTimeOffset FillTime,
    decimal Price,
    decimal Quantity,
    decimal Commission,
    decimal Slippage);

public static class DeterministicFillModel
{
    public static SimulationFill FillAtNextEligibleEvent(
        SimulationIntent intent,
        MarketEvent market,
        decimal commissionPerUnit,
        decimal slippagePerUnit)
    {
        intent.ValidateResearchOnly();
        market.Validate();

        if (market.Timestamp < intent.EarliestFillTime)
            throw new InvalidOperationException(
                "Market event is not eligible to fill the simulation intent.");

        if (commissionPerUnit < 0 || slippagePerUnit < 0)
            throw new InvalidOperationException(
                "Commission and slippage must be non-negative.");

        var direction = intent.Side == SimulationSide.Buy ? 1m : -1m;
        var fillPrice = market.Open + direction * slippagePerUnit;

        return new SimulationFill(
            intent.StrategyId,
            intent.LedgerNamespace,
            market.Timestamp,
            fillPrice,
            intent.Quantity,
            commissionPerUnit * intent.Quantity,
            slippagePerUnit * intent.Quantity);
    }
}