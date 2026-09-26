using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class AndroidLaunchGuardSummaryTests
{
    [Fact]
    public void Create_IsDeterministicAndResearchOnly()
    {
        var left = AndroidLaunchGuardSummaryRules.Create("SOURCE-A", "Ready");
        var right = AndroidLaunchGuardSummaryRules.Create("SOURCE-A", "Ready");
        Assert.Equal(left.Fingerprint, right.Fingerprint);
        Assert.False(left.CanSubmitOrders);
        AndroidLaunchGuardSummaryRules.Validate(left);
    }

    [Fact]
    public void Validate_RejectsOrderAuthority()
    {
        var value = AndroidLaunchGuardSummaryRules.Create("SOURCE-A", "Ready") with { CanSubmitOrders = true };
        Assert.Throws<InvalidOperationException>(() => AndroidLaunchGuardSummaryRules.Validate(value));
    }
}
