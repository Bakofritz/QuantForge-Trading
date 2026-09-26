using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ReleaseBoundaryTests
{
    [Fact]
    public void Create_IsDeterministicAndResearchOnly()
    {
        var left = ReleaseBoundaryRules.Create("SOURCE-A", "Ready");
        var right = ReleaseBoundaryRules.Create("SOURCE-A", "Ready");
        Assert.Equal(left.Fingerprint, right.Fingerprint);
        Assert.False(left.CanSubmitOrders);
        ReleaseBoundaryRules.Validate(left);
    }

    [Fact]
    public void Validate_RejectsOrderAuthority()
    {
        var value = ReleaseBoundaryRules.Create("SOURCE-A", "Ready") with { CanSubmitOrders = true };
        Assert.Throws<InvalidOperationException>(() => ReleaseBoundaryRules.Validate(value));
    }
}
