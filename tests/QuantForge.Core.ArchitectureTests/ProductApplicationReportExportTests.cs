using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ProductApplicationReportExportTests
{
    [Fact]
    public void Workflow_export_is_read_only_and_deterministic()
    {
        var report = new ResearchReport("job", ResearchResultStatus.DataBlocked, "dataset", "strategy", "timing", "params", "wf", "blocked", null, null);
        var summary = new ResearchWorkflowSummary(
            "workflow-sha", ResearchBatchMode.ReadOnlyResearch, 1, 0, 1, 0, 0m, 0m,
            new[] { report }, new[] { new DataReliabilityAssessment("dataset", false, 1, 0, "blocked") });
        var first = ProductApplicationReportExport.ExportWorkflow(summary);
        var second = ProductApplicationReportExport.ExportWorkflow(summary);
        Assert.Equal(first, second);
        Assert.Contains("workflow-sha", first, StringComparison.Ordinal);
    }
}
