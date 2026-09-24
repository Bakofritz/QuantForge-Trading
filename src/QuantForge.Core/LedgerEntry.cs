namespace QuantForge.Core;

public readonly record struct LedgerEntry(
    string LedgerNamespace,
    string StrategyId,
    DateTimeOffset Timestamp,
    string EventType,
    decimal CashDelta,
    decimal RealizedPnl,
    string EvidenceFingerprint);

public static class LedgerEntryRules
{
    public static void RequireComplete(LedgerEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.LedgerNamespace) ||
            string.IsNullOrWhiteSpace(entry.StrategyId) ||
            string.IsNullOrWhiteSpace(entry.EventType) ||
            string.IsNullOrWhiteSpace(entry.EvidenceFingerprint))
            throw new InvalidOperationException("Ledger evidence identity is required.");
    }
}