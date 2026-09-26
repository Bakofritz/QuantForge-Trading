using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class AndroidAcceptanceSnapshotTests
{
    private static AndroidBuildIdentity Identity => new("0.30.32", "3032", "Android", "16");

    [Fact]
    public void Snapshot_RoundTripsAndRestoresEveryMilestone()
    {
        var tracker = new AndroidAcceptanceTracker();
        tracker.Record(AndroidTestMilestone.MixedBatchInspection, true);
        tracker.Record(AndroidTestMilestone.CrossValidation, true);
        tracker.Record(AndroidTestMilestone.MinuteInspection, false);
        var snapshot = AndroidAcceptanceSnapshot.Create(Identity, tracker.States);

        Assert.True(snapshot.IsValid());
        Assert.True(AndroidAcceptanceSnapshot.TryParse(snapshot.Serialize(), out var recovered));
        Assert.NotNull(recovered);
        Assert.True(recovered!.Matches(Identity));

        var restored = new AndroidAcceptanceTracker();
        restored.Restore(recovered.States);
        Assert.Equal(AndroidTestMilestoneState.Passed, restored.States[AndroidTestMilestone.MixedBatchInspection]);
        Assert.Equal(AndroidTestMilestoneState.Passed, restored.States[AndroidTestMilestone.CrossValidation]);
        Assert.Equal(AndroidTestMilestoneState.Failed, restored.States[AndroidTestMilestone.MinuteInspection]);
        Assert.Equal(AndroidTestMilestoneState.NotRun, restored.States[AndroidTestMilestone.SourceComparison]);
    }

    [Fact]
    public void Snapshot_RejectsTamperAndDifferentBuildIdentity()
    {
        var tracker = new AndroidAcceptanceTracker();
        var snapshot = AndroidAcceptanceSnapshot.Create(Identity, tracker.States);
        var tampered = snapshot.Serialize().Replace("MinuteInspection=NotRun", "MinuteInspection=Passed", StringComparison.Ordinal);
        Assert.False(AndroidAcceptanceSnapshot.TryParse(tampered, out _));
        Assert.False(snapshot.Matches(Identity with { Build = "3033" }));
    }

    [Fact]
    public void Tracker_ResetClearsRestoredProgress()
    {
        var tracker = new AndroidAcceptanceTracker();
        tracker.Record(AndroidTestMilestone.SourceComparison, true);
        tracker.Reset();
        Assert.All(tracker.States.Values, state => Assert.Equal(AndroidTestMilestoneState.NotRun, state));
    }
}
