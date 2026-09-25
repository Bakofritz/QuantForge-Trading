namespace QuantForge.Core;

public readonly record struct SimulatedPosition(decimal Quantity, decimal AveragePrice);

public readonly record struct AccountSnapshot(
    string LedgerNamespace,
    decimal StartingCash,
    decimal Cash,
    SimulatedPosition Position,
    decimal MarketPrice,
    decimal RealizedPnl,
    decimal UnrealizedPnl,
    decimal Equity);

public sealed class SimulatedAccount
{
    private decimal _cash;
    private decimal _realizedPnl;
    private decimal _quantity;
    private decimal _averagePrice;

    public SimulatedAccount(string ledgerNamespace, decimal startingCash)
    {
        if (string.IsNullOrWhiteSpace(ledgerNamespace))
            throw new InvalidOperationException("Ledger namespace is required.");
        if (startingCash < 0)
            throw new InvalidOperationException("Starting cash must be non-negative.");

        LedgerNamespace = ledgerNamespace;
        StartingCash = startingCash;
        _cash = startingCash;
    }

    public string LedgerNamespace { get; }
    public decimal StartingCash { get; }

    public AccountSnapshot ApplyFill(SimulationFill fill)
    {
        if (fill.LedgerNamespace != LedgerNamespace)
            throw new InvalidOperationException("Fill ledger namespace does not match the account.");
        if (fill.Quantity <= 0 || fill.Price < 0 || fill.Commission < 0 || fill.Slippage < 0)
            throw new InvalidOperationException("Fill economics are invalid.");

        if (fill.Side == SimulationSide.Buy)
            ApplyBuy(fill.Quantity, fill.Price, fill.Commission);
        else
            ApplySell(fill.Quantity, fill.Price, fill.Commission);

        return Snapshot(fill.Price);
    }

    public AccountSnapshot Snapshot(decimal marketPrice)
    {
        if (marketPrice < 0)
            throw new InvalidOperationException("Market price must be non-negative.");

        var unrealized = _quantity * (marketPrice - _averagePrice);
        return new AccountSnapshot(
            LedgerNamespace,
            StartingCash,
            _cash,
            new SimulatedPosition(_quantity, _averagePrice),
            marketPrice,
            _realizedPnl,
            unrealized,
            _cash + (_quantity * marketPrice));
    }

    private void ApplyBuy(decimal quantity, decimal price, decimal commission)
    {
        var newQuantity = _quantity + quantity;
        _averagePrice = newQuantity == 0
            ? 0
            : ((_quantity * _averagePrice) + (quantity * price)) / newQuantity;
        _quantity = newQuantity;
        _cash -= (quantity * price) + commission;
    }

    private void ApplySell(decimal quantity, decimal price, decimal commission)
    {
        if (quantity > _quantity)
            throw new InvalidOperationException("Simulation account cannot sell more than its long position.");

        _cash += (quantity * price) - commission;
        _realizedPnl += quantity * (price - _averagePrice) - commission;
        _quantity -= quantity;
        if (_quantity == 0)
            _averagePrice = 0;
    }
}
