using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class AndroidResearchRecoveryPlanTests
{
    [Fact]
    public void Create_IsDeterministicAndResearchOnly()
    {
        var left = AndroidResearchRecoveryPlanRules.Create("SOURCE-A", "Ready");
        var right = AndroidResearchRecoveryPlanRules.Create("SOURCE-A", "Ready");
        Assert.Equal(left.Fingerprint, right.Fingerprint);
        Assert.False(left.CanSubmitOrders);
        AndroidResearchRecoveryPlanRules.Validate(left);
    }

    [Fact]
    public void Validate_RejectsOrderAuthority()
    {
        var value = AndroidResearchRecoveryPlanRules.Create("SOURCE-A", "Ready") with { CanSubmitOrders = true };
        Assert.Throws<InvalidOperationException>(() => AndroidResearchRecoveryPlanRules.Validate(value));
    }
}
