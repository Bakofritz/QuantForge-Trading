using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core;

/// <summary>Creates deterministic read-only session timeline entries from queue/session state.</summary>
public sealed record AndroidResearchSessionTimeline(string Fingerprint, string SourceFingerprint, string State, bool CanSubmitOrders);

public static class AndroidResearchSessionTimelineRules
{
    public static AndroidResearchSessionTimeline Create(string sourceFingerprint, string state)
    {
        if (string.IsNullOrWhiteSpace(sourceFingerprint)) throw new ArgumentException("Source fingerprint is required.", nameof(sourceFingerprint));
        if (string.IsNullOrWhiteSpace(state)) throw new ArgumentException("State is required.", nameof(state));
        var canonical = $"AndroidResearchSessionTimeline|{sourceFingerprint.Trim()}|{state.Trim()}|research-only";
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
        return new AndroidResearchSessionTimeline(fingerprint, sourceFingerprint.Trim(), state.Trim(), false);
    }

    public static void Validate(AndroidResearchSessionTimeline value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var expected = Create(value.SourceFingerprint, value.State);
        if (!string.Equals(expected.Fingerprint, value.Fingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("AndroidResearchSessionTimeline fingerprint mismatch.");
        if (value.CanSubmitOrders) throw new InvalidOperationException("Research evidence cannot grant order authority.");
    }
}
