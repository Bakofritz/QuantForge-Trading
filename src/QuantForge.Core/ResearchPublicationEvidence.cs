namespace QuantForge.Core;

public sealed record ResearchPublicationEvidence(
    ResearchPublicationBundle Publication,
    SessionCoverageReport SessionCoverage);

public static class ResearchPublicationEvidenceRules
{
    public static void Validate(ResearchPublicationEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        ResearchReportRules.Validate(evidence.Publication.Report);
        DataReliabilityRules.Validate(evidence.Publication.Reliability);
        SessionCoverageRules.RequireResearchAdmissible(evidence.SessionCoverage);

        var dataset = evidence.Publication.Report.DatasetFingerprint;
        if (!string.Equals(dataset, evidence.Publication.Reliability.DatasetFingerprint, StringComparison.Ordinal) ||
            !string.Equals(dataset, evidence.SessionCoverage.DatasetFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Publication evidence must bind reliability and session coverage to the same dataset.");
    }

    public static ResearchPublicationEvidence Bind(
        ResearchPublicationBundle publication,
        SessionCoverageReport sessionCoverage)
    {
        var evidence = new ResearchPublicationEvidence(publication, sessionCoverage);
        Validate(evidence);
        return evidence;
    }
}
