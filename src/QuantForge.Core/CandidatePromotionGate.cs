using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core;

/// <summary>Requires explicit Android and Windows native evidence before eligibility.</summary>
public sealed record CandidatePromotionGate(string Fingerprint, string SourceFingerprint, string State, bool CanSubmitOrders);

public static class CandidatePromotionGateRules
{
    public static CandidatePromotionGate Create(string sourceFingerprint, string state)
    {
        if (string.IsNullOrWhiteSpace(sourceFingerprint)) throw new ArgumentException("Source fingerprint is required.", nameof(sourceFingerprint));
        if (string.IsNullOrWhiteSpace(state)) throw new ArgumentException("State is required.", nameof(state));
        var canonical = $"CandidatePromotionGate|{sourceFingerprint.Trim()}|{state.Trim()}|research-only";
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
        return new CandidatePromotionGate(fingerprint, sourceFingerprint.Trim(), state.Trim(), false);
    }

    public static void Validate(CandidatePromotionGate value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var expected = Create(value.SourceFingerprint, value.State);
        if (!string.Equals(expected.Fingerprint, value.Fingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("CandidatePromotionGate fingerprint mismatch.");
        if (value.CanSubmitOrders) throw new InvalidOperationException("Research evidence cannot grant order authority.");
    }
}
