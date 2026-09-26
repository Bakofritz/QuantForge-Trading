using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchApplicationRecoveryTests
{
    [Fact]
    public void Recovery_exposes_only_revalidated_read_only_summary()
    {
        var root = Path.Combine(Path.GetTempPath(), "qf-recovery-" + Guid.NewGuid().ToString("N"));
        try
        {
            var summary = new ResearchWorkflowSummary(
                "workflow-sha", ResearchBatchMode.ReadOnlyResearch, 1, 0, 1, 0, 0m, 0m,
                new[] { new ResearchReport("job", ResearchResultStatus.DataBlocked, "dataset", "strategy", "timing", "params", "wf", "blocked", null, null) },
                new[] { new DataReliabilityAssessment("dataset", false, 1, 0, "blocked") });
            new ResearchWorkflowSummaryFileStore(Path.Combine(root, "workflows")).Save(summary);
            var recovered = ResearchApplicationRecovery.Load(root, "workflow-sha", Array.Empty<string>());
            Assert.Equal("workflow-sha", recovered.UiState.WorkflowFingerprint);
            Assert.False(recovered.UiState.LiveAccountEnabled);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}
