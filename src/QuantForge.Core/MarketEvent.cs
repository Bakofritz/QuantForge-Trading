namespace QuantForge.Core;

public readonly record struct MarketEvent(
    long Sequence,
    DateTimeOffset Timestamp,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume)
{
    public void Validate()
    {
        if (Sequence < 0)
            throw new InvalidOperationException("Market-event sequence must be non-negative.");

        if (High < Math.Max(Open, Close) || Low > Math.Min(Open, Close))
            throw new InvalidOperationException("OHLC relationship is invalid.");

        if (Low < 0 || Open < 0 || High < 0 || Close < 0)
            throw new InvalidOperationException("Negative prices are not admitted.");

        if (Volume < 0)
            throw new InvalidOperationException("Negative volume is not admitted.");
    }
}