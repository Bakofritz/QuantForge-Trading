namespace QuantForge.Core;

public static class ProductApplicationReportExport
{
    public static string ExportWorkflow(ResearchWorkflowSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);
        var state = ProductUiBoundary.CreateReadOnlyState(summary);
        if (state.LiveAccountEnabled || state.CanSubmitOrders || state.CanChangeApplicationSettings)
            throw new InvalidOperationException("Report export cannot originate from an authority-escalated application state.");
        return ResearchWorkflowMarkdownExporter.Export(summary);
    }

    public static string ExportReport(ResearchReport report)
    {
        ResearchReportRules.Validate(report);
        return string.Join("\n", new[]
        {
            "# QuantForge Research Report",
            $"- Job: `{report.JobId}`",
            $"- Status: `{report.Status}`",
            $"- Dataset: `{report.DatasetFingerprint}`",
            $"- Strategy: `{report.StrategyFingerprint}`",
            $"- Evidence: `{report.EvidenceTail?.Fingerprint ?? "n/a"}`",
            $"- Reason: {report.BlockReason ?? "n/a"}"
        }) + "\n";
    }
}
