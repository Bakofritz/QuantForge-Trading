namespace QuantForge.Core;

public enum ResearchResultStatus
{
    Complete,
    DataBlocked,
    Invalid
}

public readonly record struct ResearchReport(
    string JobId,
    ResearchResultStatus Status,
    string DatasetFingerprint,
    string StrategyFingerprint,
    string ExecutionPolicyFingerprint,
    string ParameterFingerprint,
    string TemporalPartition,
    string? BlockReason,
    AccountSnapshot? Account);

public static class ResearchReportRules
{
    public static void Validate(ResearchReport report)
    {
        if (string.IsNullOrWhiteSpace(report.JobId) ||
            string.IsNullOrWhiteSpace(report.DatasetFingerprint) ||
            string.IsNullOrWhiteSpace(report.StrategyFingerprint) ||
            string.IsNullOrWhiteSpace(report.ExecutionPolicyFingerprint) ||
            string.IsNullOrWhiteSpace(report.ParameterFingerprint) ||
            string.IsNullOrWhiteSpace(report.TemporalPartition))
            throw new InvalidOperationException("Research report reproducibility identity is incomplete.");

        if (report.Status == ResearchResultStatus.DataBlocked && string.IsNullOrWhiteSpace(report.BlockReason))
            throw new InvalidOperationException("Data-blocked reports require a block reason.");

        if (report.Status == ResearchResultStatus.Complete && report.Account is null)
            throw new InvalidOperationException("Complete simulation reports require an account snapshot.");

        if (report.Status != ResearchResultStatus.Complete && report.Account is not null)
            throw new InvalidOperationException("Blocked or invalid reports cannot contain simulated performance state.");
    }
}
