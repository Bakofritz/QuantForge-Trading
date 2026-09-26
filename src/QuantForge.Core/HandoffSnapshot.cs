using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core;

/// <summary>Creates deterministic handoff state for a new orchestrator session.</summary>
public sealed record HandoffSnapshot(string Fingerprint, string SourceFingerprint, string State, bool CanSubmitOrders);

public static class HandoffSnapshotRules
{
    public static HandoffSnapshot Create(string sourceFingerprint, string state)
    {
        if (string.IsNullOrWhiteSpace(sourceFingerprint)) throw new ArgumentException("Source fingerprint is required.", nameof(sourceFingerprint));
        if (string.IsNullOrWhiteSpace(state)) throw new ArgumentException("State is required.", nameof(state));
        var canonical = $"HandoffSnapshot|{sourceFingerprint.Trim()}|{state.Trim()}|research-only";
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
        return new HandoffSnapshot(fingerprint, sourceFingerprint.Trim(), state.Trim(), false);
    }

    public static void Validate(HandoffSnapshot value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var expected = Create(value.SourceFingerprint, value.State);
        if (!string.Equals(expected.Fingerprint, value.Fingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("HandoffSnapshot fingerprint mismatch.");
        if (value.CanSubmitOrders) throw new InvalidOperationException("Research evidence cannot grant order authority.");
    }
}
