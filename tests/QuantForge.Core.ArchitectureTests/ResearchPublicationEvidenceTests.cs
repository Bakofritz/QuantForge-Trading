using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchPublicationEvidenceTests
{
    [Fact]
    public void Publication_evidence_requires_same_dataset_for_coverage_and_reliability()
    {
        var reliability = new DataReliabilityAssessment("dataset-sha", true, 0, 0, "validated");
        var coverage = new SessionCoverageReport(
            "other-dataset", "policy-sha", 2, 2, 0, 0,
            Array.Empty<DateTimeOffset>(), Array.Empty<DateTimeOffset>(), true, "authoritative");

        var report = new ResearchReport(
            "job-sha", ResearchResultStatus.DataBlocked, "dataset-sha", "strategy-sha",
            "timing-sha", "parameters-sha", "walk-forward-01", "blocked", null, null);
        var publication = new ResearchPublicationBundle(report,
            new ResearchProvenanceBundle(default, "dataset-sha", "job-sha", "evidence-sha"), reliability);

        Assert.Throws<InvalidOperationException>(() => ResearchPublicationEvidenceRules.Bind(publication, coverage));
    }

    [Fact]
    public void Non_authoritative_coverage_cannot_be_publication_evidence()
    {
        var coverage = new SessionCoverageReport(
            "dataset-sha", "policy-sha", 2, 2, 0, 0,
            Array.Empty<DateTimeOffset>(), Array.Empty<DateTimeOffset>(), false, "observed only");
        Assert.False(coverage.ResearchAdmissible);
        Assert.Throws<InvalidOperationException>(() => SessionCoverageRules.RequireResearchAdmissible(coverage));
    }
}
