using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class AndroidResearchSessionTimelineTests
{
    [Fact]
    public void Create_IsDeterministicAndResearchOnly()
    {
        var left = AndroidResearchSessionTimelineRules.Create("SOURCE-A", "Ready");
        var right = AndroidResearchSessionTimelineRules.Create("SOURCE-A", "Ready");
        Assert.Equal(left.Fingerprint, right.Fingerprint);
        Assert.False(left.CanSubmitOrders);
        AndroidResearchSessionTimelineRules.Validate(left);
    }

    [Fact]
    public void Validate_RejectsOrderAuthority()
    {
        var value = AndroidResearchSessionTimelineRules.Create("SOURCE-A", "Ready") with { CanSubmitOrders = true };
        Assert.Throws<InvalidOperationException>(() => AndroidResearchSessionTimelineRules.Validate(value));
    }
}
