namespace QuantForge.Core;

public sealed record ResearchPublicationBundle(
    ResearchReport Report,
    ResearchProvenanceBundle Provenance,
    DataReliabilityAssessment Reliability);

public static class ResearchPublicationRules
{
    public static ResearchPublicationBundle Bind(
        ResearchJobSpec job,
        ProvenanceRecord strategySource,
        ResearchReport report,
        DataReliabilityAssessment reliability)
    {
        ResearchReportRules.Validate(report);
        DataReliabilityRules.Validate(reliability);

        if (report.Status != ResearchResultStatus.Complete)
            throw new InvalidOperationException("Only complete research results can be published.");

        if (!DataReliabilityRules.IsResearchAdmissible(reliability))
            throw new InvalidOperationException("Publishable research requires reliability-admissible data with no unresolved gaps or conflicting overlaps.");

        if (!string.Equals(reliability.DatasetFingerprint, report.DatasetFingerprint, StringComparison.Ordinal) ||
            !string.Equals(reliability.DatasetFingerprint, job.Identity.DatasetFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Publication reliability identity does not match the research dataset.");

        var provenance = ResearchProvenanceRules.Bind(job, strategySource, report);
        return new ResearchPublicationBundle(report, provenance, reliability);
    }
}
