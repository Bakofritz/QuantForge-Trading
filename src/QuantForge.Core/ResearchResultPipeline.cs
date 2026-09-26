namespace QuantForge.Core;

public sealed record ResearchResultPipelineOutput(
    ResearchReport Report,
    ResearchExecutionTrace? Execution,
    string? ChartData,
    string? LedgerData);

public static class ResearchResultPipeline
{
    public static ResearchResultPipelineOutput Run(ResearchRunRequest request)
    {
        var report = DeterministicResearchRunner.Run(request);
        if (report.Status != ResearchResultStatus.Complete || report.ExecutionTrace is null)
            return new ResearchResultPipelineOutput(report, null, null, null);

        ResearchExecutionTraceRules.Validate(report.ExecutionTrace);
        var chart = ExportChartCsv(report.ExecutionTrace);
        var ledger = ExportLedgerCsv(report.ExecutionTrace);
        return new ResearchResultPipelineOutput(report, report.ExecutionTrace, chart, ledger);
    }

    public static ResearchResultPipelineOutput FromReport(ResearchReport report)
    {
        ResearchReportRules.Validate(report);
        if (report.Status != ResearchResultStatus.Complete || report.ExecutionTrace is null)
            return new ResearchResultPipelineOutput(report, null, null, null);

        ResearchExecutionTraceRules.Validate(report.ExecutionTrace.Value);
        return new ResearchResultPipelineOutput(
            report,
            report.ExecutionTrace,
            ExportChartCsv(report.ExecutionTrace.Value),
            ExportLedgerCsv(report.ExecutionTrace.Value));
    }

    private static string ExportChartCsv(ResearchExecutionTrace trace)
    {
        var lines = new List<string> { "timestamp,equity,cash,position_quantity,market_price" };
        lines.AddRange(trace.EquityCurve.Select(x =>
            string.Join(",", x.Timestamp.ToString("O"), x.Equity, x.Cash, x.PositionQuantity, x.MarketPrice)));
        return string.Join("\n", lines);
    }

    private static string ExportLedgerCsv(ResearchExecutionTrace trace)
    {
        var lines = new List<string> { "ledger_namespace,strategy_id,timestamp,event_type,cash_delta,realized_pnl,evidence_fingerprint" };
        lines.AddRange(trace.Ledger.Select(x =>
            string.Join(",", Csv(x.LedgerNamespace), Csv(x.StrategyId), x.Timestamp.ToString("O"), Csv(x.EventType), x.CashDelta, x.RealizedPnl, x.EvidenceFingerprint)));
        return string.Join("\n", lines);
    }

    private static string Csv(string value) =>
        $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
