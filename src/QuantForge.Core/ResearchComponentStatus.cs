namespace QuantForge.Core;

public enum ResearchComponentState
{
    Pending,
    Running,
    Complete,
    DataBlocked,
    Invalid
}

public sealed record ResearchComponentStatus(
    string JobFingerprint,
    ResearchComponentState State,
    string? Message);

public static class ResearchComponentStatusRules
{
    public static ResearchComponentStatus FromReport(ResearchReport report)
    {
        ResearchReportRules.Validate(report);

        return report.Status switch
        {
            ResearchResultStatus.Complete =>
                new(report.JobId, ResearchComponentState.Complete, null),
            ResearchResultStatus.DataBlocked =>
                new(report.JobId, ResearchComponentState.DataBlocked, report.BlockReason),
            ResearchResultStatus.Invalid =>
                new(report.JobId, ResearchComponentState.Invalid, report.BlockReason),
            _ => throw new InvalidOperationException("Unknown research result state.")
        };
    }

    public static void RequireTerminal(ResearchComponentStatus status)
    {
        if (string.IsNullOrWhiteSpace(status.JobFingerprint))
            throw new InvalidOperationException("Component status requires research job identity.");

        if (status.State is ResearchComponentState.Pending or ResearchComponentState.Running)
            throw new InvalidOperationException("Research component has not reached a terminal state.");

        if (status.State is ResearchComponentState.DataBlocked or ResearchComponentState.Invalid &&
            string.IsNullOrWhiteSpace(status.Message))
            throw new InvalidOperationException("Blocked or invalid component state requires an explicit reason.");
    }
}
