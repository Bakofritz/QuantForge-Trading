using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core;

/// <summary>Builds a fail-closed recovery plan for interrupted research sessions.</summary>
public sealed record AndroidResearchRecoveryPlan(string Fingerprint, string SourceFingerprint, string State, bool CanSubmitOrders);

public static class AndroidResearchRecoveryPlanRules
{
    public static AndroidResearchRecoveryPlan Create(string sourceFingerprint, string state)
    {
        if (string.IsNullOrWhiteSpace(sourceFingerprint)) throw new ArgumentException("Source fingerprint is required.", nameof(sourceFingerprint));
        if (string.IsNullOrWhiteSpace(state)) throw new ArgumentException("State is required.", nameof(state));
        var canonical = $"AndroidResearchRecoveryPlan|{sourceFingerprint.Trim()}|{state.Trim()}|research-only";
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
        return new AndroidResearchRecoveryPlan(fingerprint, sourceFingerprint.Trim(), state.Trim(), false);
    }

    public static void Validate(AndroidResearchRecoveryPlan value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var expected = Create(value.SourceFingerprint, value.State);
        if (!string.Equals(expected.Fingerprint, value.Fingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("AndroidResearchRecoveryPlan fingerprint mismatch.");
        if (value.CanSubmitOrders) throw new InvalidOperationException("Research evidence cannot grant order authority.");
    }
}
