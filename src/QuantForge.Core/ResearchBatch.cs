namespace QuantForge.Core;

public sealed record ResearchStrategyInstance(
    string StrategyId,
    string StrategyFingerprint,
    LedgerNamespace LedgerNamespace);

public static class ResearchBatchRules
{
    public static void RequireIndependentStrategies(
        IReadOnlyList<ResearchStrategyInstance> strategies)
    {
        if (strategies.Count == 0)
            throw new InvalidOperationException("A research batch requires at least one strategy.");

        var strategyIds = strategies.Select(x => x.StrategyId).ToArray();
        if (strategyIds.Any(string.IsNullOrWhiteSpace) ||
            strategyIds.Distinct(StringComparer.Ordinal).Count() != strategyIds.Length)
            throw new InvalidOperationException(
                "Strategy IDs must be unique within a research batch.");

        var ledgerIds = strategies
            .Select(x => $"{x.LedgerNamespace.BatchId}|{x.LedgerNamespace.StrategyId}|{x.LedgerNamespace.AccountId}")
            .ToArray();

        if (ledgerIds.Distinct(StringComparer.Ordinal).Count() != ledgerIds.Length)
            throw new InvalidOperationException(
                "Each strategy requires an independent ledger namespace.");

        foreach (var strategy in strategies)
        {
            if (string.IsNullOrWhiteSpace(strategy.StrategyFingerprint))
                throw new InvalidOperationException("Strategy fingerprint is required.");
        }
    }
}