namespace QuantForge.Core;

public readonly record struct LedgerNamespace(
    string BatchId,
    string StrategyId,
    string AccountId);

public static class LedgerIsolationRules
{
    public static void RequireDistinct(
        LedgerNamespace first,
        LedgerNamespace second)
    {
        if (string.IsNullOrWhiteSpace(first.BatchId) ||
            string.IsNullOrWhiteSpace(first.StrategyId) ||
            string.IsNullOrWhiteSpace(first.AccountId) ||
            string.IsNullOrWhiteSpace(second.BatchId) ||
            string.IsNullOrWhiteSpace(second.StrategyId) ||
            string.IsNullOrWhiteSpace(second.AccountId))
            throw new InvalidOperationException("Ledger namespaces must be complete.");

        if (first == second)
            throw new InvalidOperationException(
                "Independent strategies cannot share an identical simulated ledger namespace.");
    }
}