using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class AndroidDeviceTestReportTests
{
    [Fact]
    public void Render_BindsEveryMilestoneToExactBuildWithoutAuthority()
    {
        var tracker = new AndroidAcceptanceTracker();
        tracker.Record(AndroidTestMilestone.MixedBatchInspection, true);
        var report = AndroidDeviceTestReport.Render(new AndroidBuildIdentity("0.30.31", "3031", "Android", "16"), tracker.States);

        Assert.Contains("Version: 0.30.31 (3031)", report, StringComparison.Ordinal);
        Assert.Contains("MixedBatchInspection: Passed", report, StringComparison.Ordinal);
        Assert.Contains("SourceComparison: NotRun", report, StringComparison.Ordinal);
        Assert.Contains("no research admission", report, StringComparison.Ordinal);
        Assert.Contains("no research admission, broker connection, live account, or order authority", report, StringComparison.Ordinal);
    }
}
