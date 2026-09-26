using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core;

/// <summary>Records native validation result without promoting stability.</summary>
public sealed record NativeValidationReceipt(string Fingerprint, string SourceFingerprint, string State, bool CanSubmitOrders);

public static class NativeValidationReceiptRules
{
    public static NativeValidationReceipt Create(string sourceFingerprint, string state)
    {
        if (string.IsNullOrWhiteSpace(sourceFingerprint)) throw new ArgumentException("Source fingerprint is required.", nameof(sourceFingerprint));
        if (string.IsNullOrWhiteSpace(state)) throw new ArgumentException("State is required.", nameof(state));
        var canonical = $"NativeValidationReceipt|{sourceFingerprint.Trim()}|{state.Trim()}|research-only";
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
        return new NativeValidationReceipt(fingerprint, sourceFingerprint.Trim(), state.Trim(), false);
    }

    public static void Validate(NativeValidationReceipt value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var expected = Create(value.SourceFingerprint, value.State);
        if (!string.Equals(expected.Fingerprint, value.Fingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("NativeValidationReceipt fingerprint mismatch.");
        if (value.CanSubmitOrders) throw new InvalidOperationException("Research evidence cannot grant order authority.");
    }
}
