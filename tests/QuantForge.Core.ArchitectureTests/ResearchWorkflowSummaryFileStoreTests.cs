using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchWorkflowSummaryFileStoreTests
{
    [Fact]
    public void Workflow_summary_store_is_idempotent_for_same_fingerprint()
    {
        var summary = new ResearchWorkflowSummary(
            "workflow-sha", ResearchBatchMode.ReadOnlyResearch, 1, 0, 1, 0, 0m, 0m,
            new[] { new ResearchReport("job", ResearchResultStatus.DataBlocked, "dataset", "strategy", "timing", "params", "wf", "blocked", null, null) },
            new[] { new DataReliabilityAssessment("dataset", false, 1, 0, "blocked") });
        var root = Path.Combine(Path.GetTempPath(), "qf-summary-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new ResearchWorkflowSummaryFileStore(root);
            var first = store.Save(summary);
            var second = store.Save(summary);
            Assert.Equal(first, second);
            Assert.True(File.Exists(first));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}
