namespace QuantForge.Core;

public enum SimulationSide
{
    Buy,
    Sell
}

public enum SimulationIntentType
{
    Market,
    Limit,
    Stop
}

public readonly record struct SimulationIntent(
    string StrategyId,
    string LedgerNamespace,
    SimulationSide Side,
    SimulationIntentType Type,
    decimal Quantity,
    DateTimeOffset SignalTime,
    DateTimeOffset EarliestFillTime)
{
    public void ValidateResearchOnly()
    {
        if (string.IsNullOrWhiteSpace(StrategyId) ||
            string.IsNullOrWhiteSpace(LedgerNamespace))
            throw new InvalidOperationException("Simulation intents require isolated strategy and ledger identities.");

        if (Quantity <= 0)
            throw new InvalidOperationException("Simulation quantity must be positive.");

        if (EarliestFillTime < SignalTime)
            throw new InvalidOperationException("A simulated fill cannot precede its signal time.");
    }
}