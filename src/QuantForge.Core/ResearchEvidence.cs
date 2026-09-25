namespace QuantForge.Core;

public static class ResearchEvidence
{
    public static EvidenceRecord CreateRoot(ResearchJobIdentity identity)
    {
        if (string.IsNullOrWhiteSpace(identity.JobFingerprint))
            throw new InvalidOperationException("Research job identity is required.");

        var payloadFingerprint = EvidenceChain.Sha256(identity.JobFingerprint);
        var fingerprint = EvidenceChain.Sha256(
            $"0||research-root|{payloadFingerprint}");

        return new EvidenceRecord(
            0,
            string.Empty,
            "research-root",
            payloadFingerprint,
            fingerprint);
    }

    public static EvidenceRecord AppendFill(
        EvidenceRecord previous,
        long sequence,
        SimulationFill fill)
    {
        var payload =
            $"{fill.StrategyId}|{fill.LedgerNamespace}|{fill.Side}|{fill.FillTime:O}|{fill.Price}|{fill.Quantity}|{fill.Commission}|{fill.Slippage}";

        return EvidenceChain.Append(previous, sequence, "simulation-fill", payload);
    }
}
