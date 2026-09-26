using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class AndroidAcceptanceTrackerTests
{
    [Fact]
    public void Tracker_SeparatesPassedFailedAndNotRunWithoutGrantingAdmission()
    {
        var tracker = new AndroidAcceptanceTracker();
        tracker.Record(AndroidTestMilestone.MixedBatchInspection, true);
        tracker.Record(AndroidTestMilestone.SourceComparison, false);

        var text = tracker.Render();

        Assert.Contains("1 passed", text, StringComparison.Ordinal);
        Assert.Contains("1 failed", text, StringComparison.Ordinal);
        Assert.Contains("2 not run", text, StringComparison.Ordinal);
        Assert.Contains("not research admission", text, StringComparison.Ordinal);
    }
}
