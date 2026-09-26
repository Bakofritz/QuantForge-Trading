namespace QuantForge.Core;

public enum ResearchOptimizationObjective
{
    FinalEquity,
    RealizedPnl
}

public sealed record ResearchOptimizationSelection(
    ResearchOptimizationObjective Objective,
    string SelectedJobId,
    string SelectionFingerprint);

public static class ResearchOptimizationSelectionRules
{
    public static ResearchOptimizationSelection Select(
        ResearchOptimizationResult result,
        ResearchOptimizationObjective objective)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (!result.Complete || result.Reports.Count == 0)
            throw new InvalidOperationException("Optimization selection requires a complete result set with every variant executable.");

        foreach (var report in result.Reports)
        {
            ResearchReportRules.Validate(report);
            if (report.Status != ResearchResultStatus.Complete || report.Account is null)
                throw new InvalidOperationException("Optimization selection requires complete simulated account state for every variant.");
        }

        var ordered = result.Reports
            .OrderByDescending(x => ObjectiveValue(x, objective))
            .ThenBy(x => x.JobId, StringComparer.Ordinal)
            .ToArray();
        var selected = ordered[0];
        var payload = string.Join("\n", ordered.Select(x =>
            $"{x.JobId}|{x.DatasetFingerprint}|{x.StrategyFingerprint}|{x.ParameterFingerprint}|{ObjectiveValue(x, objective):G29}"));
        var fingerprint = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes($"{objective}\n{payload}")));
        return new ResearchOptimizationSelection(objective, selected.JobId, fingerprint);
    }

    private static decimal ObjectiveValue(ResearchReport report, ResearchOptimizationObjective objective) =>
        objective switch
        {
            ResearchOptimizationObjective.FinalEquity => report.Account!.Value.Equity,
            ResearchOptimizationObjective.RealizedPnl => report.Account!.Value.RealizedPnl,
            _ => throw new ArgumentOutOfRangeException(nameof(objective))
        };
}
