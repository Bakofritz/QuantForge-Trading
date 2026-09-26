using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core;

/// <summary>Binds requested native platform validation to exact source identity.</summary>
public sealed record NativeValidationRequest(string Fingerprint, string SourceFingerprint, string State, bool CanSubmitOrders);

public static class NativeValidationRequestRules
{
    public static NativeValidationRequest Create(string sourceFingerprint, string state)
    {
        if (string.IsNullOrWhiteSpace(sourceFingerprint)) throw new ArgumentException("Source fingerprint is required.", nameof(sourceFingerprint));
        if (string.IsNullOrWhiteSpace(state)) throw new ArgumentException("State is required.", nameof(state));
        var canonical = $"NativeValidationRequest|{sourceFingerprint.Trim()}|{state.Trim()}|research-only";
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
        return new NativeValidationRequest(fingerprint, sourceFingerprint.Trim(), state.Trim(), false);
    }

    public static void Validate(NativeValidationRequest value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var expected = Create(value.SourceFingerprint, value.State);
        if (!string.Equals(expected.Fingerprint, value.Fingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("NativeValidationRequest fingerprint mismatch.");
        if (value.CanSubmitOrders) throw new InvalidOperationException("Research evidence cannot grant order authority.");
    }
}
