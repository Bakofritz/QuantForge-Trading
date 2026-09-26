using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class HandoffVerifierTests
{
    [Fact]
    public void Create_IsDeterministicAndResearchOnly()
    {
        var left = HandoffVerifierRules.Create("SOURCE-A", "Ready");
        var right = HandoffVerifierRules.Create("SOURCE-A", "Ready");
        Assert.Equal(left.Fingerprint, right.Fingerprint);
        Assert.False(left.CanSubmitOrders);
        HandoffVerifierRules.Validate(left);
    }

    [Fact]
    public void Validate_RejectsOrderAuthority()
    {
        var value = HandoffVerifierRules.Create("SOURCE-A", "Ready") with { CanSubmitOrders = true };
        Assert.Throws<InvalidOperationException>(() => HandoffVerifierRules.Validate(value));
    }
}
