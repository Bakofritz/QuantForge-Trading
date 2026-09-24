using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public class ResearchReportTests
{
    [Fact]
    public void Data_blocked_report_requires_reason_and_has_no_performance_state()
    {
        var report = new ResearchReport(
            "job-1", ResearchResultStatus.DataBlocked,
            "data-sha", "strategy-sha", "exec-sha", "params-sha", "2026-Q1",
            "authoritative market-data coverage is incomplete", null);

        ResearchReportRules.Validate(report);
    }

    [Fact]
    public void Complete_report_requires_account_state()
    {
        var report = new ResearchReport(
            "job-1", ResearchResultStatus.Complete,
            "data-sha", "strategy-sha", "exec-sha", "params-sha", "2026-Q1",
            null, null);

        Assert.Throws<InvalidOperationException>(() => ResearchReportRules.Validate(report));
    }
}
