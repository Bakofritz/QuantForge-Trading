using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchWorkflowSummaryFileStoreLoadTests
{
    [Fact]
    public void Persisted_summary_can_be_reloaded_and_rehashed()
    {
        var summary = new ResearchWorkflowSummary(
            "workflow-sha", ResearchBatchMode.ReadOnlyResearch, 1, 0, 1, 0, 0m, 0m,
            new[] { new ResearchReport("job", ResearchResultStatus.DataBlocked, "dataset", "strategy", "timing", "params", "wf", "blocked", null, null) },
            new[] { new DataReliabilityAssessment("dataset", false, 1, 0, "blocked") });
        var root = Path.Combine(Path.GetTempPath(), "qf-summary-load-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new ResearchWorkflowSummaryFileStore(root);
            store.Save(summary);
            var loaded = store.Load("workflow-sha");
            Assert.Equal(summary.WorkflowFingerprint, loaded.WorkflowFingerprint);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}
