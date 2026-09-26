using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class HandoffSnapshotTests
{
    [Fact]
    public void Create_IsDeterministicAndResearchOnly()
    {
        var left = HandoffSnapshotRules.Create("SOURCE-A", "Ready");
        var right = HandoffSnapshotRules.Create("SOURCE-A", "Ready");
        Assert.Equal(left.Fingerprint, right.Fingerprint);
        Assert.False(left.CanSubmitOrders);
        HandoffSnapshotRules.Validate(left);
    }

    [Fact]
    public void Validate_RejectsOrderAuthority()
    {
        var value = HandoffSnapshotRules.Create("SOURCE-A", "Ready") with { CanSubmitOrders = true };
        Assert.Throws<InvalidOperationException>(() => HandoffSnapshotRules.Validate(value));
    }
}
