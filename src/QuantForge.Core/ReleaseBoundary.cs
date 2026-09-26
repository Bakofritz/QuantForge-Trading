using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core;

/// <summary>Binds the 15-iteration release boundary to source and validation evidence.</summary>
public sealed record ReleaseBoundary(string Fingerprint, string SourceFingerprint, string State, bool CanSubmitOrders);

public static class ReleaseBoundaryRules
{
    public static ReleaseBoundary Create(string sourceFingerprint, string state)
    {
        if (string.IsNullOrWhiteSpace(sourceFingerprint)) throw new ArgumentException("Source fingerprint is required.", nameof(sourceFingerprint));
        if (string.IsNullOrWhiteSpace(state)) throw new ArgumentException("State is required.", nameof(state));
        var canonical = $"ReleaseBoundary|{sourceFingerprint.Trim()}|{state.Trim()}|research-only";
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
        return new ReleaseBoundary(fingerprint, sourceFingerprint.Trim(), state.Trim(), false);
    }

    public static void Validate(ReleaseBoundary value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var expected = Create(value.SourceFingerprint, value.State);
        if (!string.Equals(expected.Fingerprint, value.Fingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("ReleaseBoundary fingerprint mismatch.");
        if (value.CanSubmitOrders) throw new InvalidOperationException("Research evidence cannot grant order authority.");
    }
}
