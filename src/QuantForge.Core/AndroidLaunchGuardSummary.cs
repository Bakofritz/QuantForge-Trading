using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core;

/// <summary>Summarizes dataset/strategy/readiness launch guards.</summary>
public sealed record AndroidLaunchGuardSummary(string Fingerprint, string SourceFingerprint, string State, bool CanSubmitOrders);

public static class AndroidLaunchGuardSummaryRules
{
    public static AndroidLaunchGuardSummary Create(string sourceFingerprint, string state)
    {
        if (string.IsNullOrWhiteSpace(sourceFingerprint)) throw new ArgumentException("Source fingerprint is required.", nameof(sourceFingerprint));
        if (string.IsNullOrWhiteSpace(state)) throw new ArgumentException("State is required.", nameof(state));
        var canonical = $"AndroidLaunchGuardSummary|{sourceFingerprint.Trim()}|{state.Trim()}|research-only";
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
        return new AndroidLaunchGuardSummary(fingerprint, sourceFingerprint.Trim(), state.Trim(), false);
    }

    public static void Validate(AndroidLaunchGuardSummary value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var expected = Create(value.SourceFingerprint, value.State);
        if (!string.Equals(expected.Fingerprint, value.Fingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("AndroidLaunchGuardSummary fingerprint mismatch.");
        if (value.CanSubmitOrders) throw new InvalidOperationException("Research evidence cannot grant order authority.");
    }
}
