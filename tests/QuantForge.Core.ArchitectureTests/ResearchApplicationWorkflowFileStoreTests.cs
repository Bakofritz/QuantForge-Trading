using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchApplicationWorkflowFileStoreTests
{
    [Fact]
    public void Empty_application_result_can_be_persisted_without_result_artifacts()
    {
        var root = Path.Combine(Path.GetTempPath(), "qf-app-store-" + Guid.NewGuid().ToString("N"));
        try
        {
            var summary = new ResearchWorkflowSummary(
                "workflow-sha", ResearchBatchMode.ReadOnlyResearch, 1, 0, 1, 0, 0m, 0m,
                new[] { new ResearchReport("job", ResearchResultStatus.DataBlocked, "dataset", "strategy", "timing", "params", "wf", "blocked", null, null) },
                new[] { new DataReliabilityAssessment("dataset", false, 1, 0, "blocked") });
            var result = new ResearchApplicationWorkflowResult(summary, Array.Empty<ResearchPublicationArtifact>());
            new ResearchApplicationWorkflowFileStore(root).Save(result);
            Assert.True(File.Exists(Path.Combine(root, "workflows", "workflow-sha", "report.md")));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}
