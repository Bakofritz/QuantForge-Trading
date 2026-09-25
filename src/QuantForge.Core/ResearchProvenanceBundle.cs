namespace QuantForge.Core;

public sealed record ResearchProvenanceBundle(
    ProvenanceRecord StrategySource,
    string DatasetFingerprint,
    string JobFingerprint,
    string EvidenceFingerprint);

public static class ResearchProvenanceRules
{
    public static ResearchProvenanceBundle Bind(
        ResearchJobSpec job,
        ProvenanceRecord strategySource,
        ResearchReport report)
    {
        ProvenanceRules.RequireComplete(strategySource);
        ResearchReportRules.Validate(report);

        if (!string.Equals(strategySource.SanitizedArtifactFingerprint, job.Strategy.SourceFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Strategy provenance does not match the admitted sanitized strategy.");

        if (!string.Equals(report.DatasetFingerprint, job.Identity.DatasetFingerprint, StringComparison.Ordinal) ||
            !string.Equals(report.StrategyFingerprint, job.Identity.StrategyFingerprint, StringComparison.Ordinal) ||
            !string.Equals(report.JobId, job.Identity.JobFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Research report provenance does not match the research job identity.");

        if (report.Status != ResearchResultStatus.Complete || report.EvidenceTail is null)
            throw new InvalidOperationException("Only complete research results can be bound as publishable provenance.");

        return new ResearchProvenanceBundle(
            strategySource,
            report.DatasetFingerprint,
            report.JobId,
            report.EvidenceTail.Value.Fingerprint);
    }
}
