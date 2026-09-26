using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core;

/// <summary>Tracks immutable strategy-draft fingerprints for Android review.</summary>
public sealed record StrategyDraftHistory(string Fingerprint, string SourceFingerprint, string State, bool CanSubmitOrders);

public static class StrategyDraftHistoryRules
{
    public static StrategyDraftHistory Create(string sourceFingerprint, string state)
    {
        if (string.IsNullOrWhiteSpace(sourceFingerprint)) throw new ArgumentException("Source fingerprint is required.", nameof(sourceFingerprint));
        if (string.IsNullOrWhiteSpace(state)) throw new ArgumentException("State is required.", nameof(state));
        var canonical = $"StrategyDraftHistory|{sourceFingerprint.Trim()}|{state.Trim()}|research-only";
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
        return new StrategyDraftHistory(fingerprint, sourceFingerprint.Trim(), state.Trim(), false);
    }

    public static void Validate(StrategyDraftHistory value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var expected = Create(value.SourceFingerprint, value.State);
        if (!string.Equals(expected.Fingerprint, value.Fingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("StrategyDraftHistory fingerprint mismatch.");
        if (value.CanSubmitOrders) throw new InvalidOperationException("Research evidence cannot grant order authority.");
    }
}
