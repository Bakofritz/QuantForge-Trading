using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core;

public readonly record struct EvidenceRecord(
    long Sequence,
    string PreviousFingerprint,
    string EventType,
    string PayloadFingerprint,
    string Fingerprint);

public static class EvidenceChain
{
    public static EvidenceRecord Append(
        EvidenceRecord previous,
        long sequence,
        string eventType,
        string payload)
    {
        if (sequence <= previous.Sequence)
            throw new InvalidOperationException("Evidence sequence must increase.");

        if (string.IsNullOrWhiteSpace(eventType) ||
            string.IsNullOrWhiteSpace(payload))
            throw new InvalidOperationException("Evidence event data is required.");

        var payloadFingerprint = Sha256(payload);
        var fingerprint = Sha256(
            $"{sequence}|{previous.Fingerprint}|{eventType}|{payloadFingerprint}");

        return new EvidenceRecord(
            sequence,
            previous.Fingerprint,
            eventType,
            payloadFingerprint,
            fingerprint);
    }

    public static string Sha256(string value)
    {
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }
}