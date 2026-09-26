using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public class ResearchReportTests
{
    [Fact]
    public void Data_blocked_report_requires_reason_and_has_no_performance_state()
    {
        var report = new ResearchReport(
            "job-1", ResearchResultStatus.DataBlocked,
            "data-sha", "strategy-sha", "exec-sha", "params-sha", "2026-Q1",
            "authoritative market-data coverage is incomplete", null, null);

        ResearchReportRules.Validate(report);
    }

    [Fact]
    public void Complete_report_requires_account_and_evidence_state()
    {
        var report = new ResearchReport(
            "job-1", ResearchResultStatus.Complete,
            "data-sha", "strategy-sha", "exec-sha", "params-sha", "2026-Q1",
            null, null, null);

        Assert.Throws<InvalidOperationException>(() => ResearchReportRules.Validate(report));
    }
}

public class ResearchPublicationArtifactTests
{
    [Fact]
    public void Archive_is_empty_and_deterministically_listable()
    {
        var archive = new ResearchResultArchive();
        Assert.Equal(0, archive.Count);
        Assert.Empty(archive.List());
    }

    [Fact]
    public void Archive_rejects_incomplete_publication()
    {
        var archive = new ResearchResultArchive();
        Assert.Throws<ArgumentNullException>(() => archive.Add(null!));
    }
}

public class ResearchWorkflowPackageTests
{
    [Fact]
    public void Workflow_package_factory_rejects_empty_publications()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ResearchWorkflowPackageFactory.Create(
                default!,
                Array.Empty<ResearchPublicationArtifact>()));
    }
}

public class ResearchPublicationFileStoreTests
{
    [Fact]
    public void File_store_requires_safe_root()
    {
        Assert.Throws<ArgumentException>(() => new ResearchPublicationFileStore(""));
    }
}

public class EndToEndResearchCoordinatorTests
{
    [Fact]
    public void Coordinator_requires_dataset_catalog()
    {
        Assert.Throws<ArgumentNullException>(() =>
            EndToEndResearchCoordinator.Run(null!));
    }
}

public class DatasetCatalogFileStoreTests
{
    [Fact]
    public void Store_requires_a_path()
    {
        Assert.Throws<ArgumentException>(() => new DatasetCatalogFileStore(""));
    }
}
