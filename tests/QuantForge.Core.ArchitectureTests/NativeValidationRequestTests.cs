using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class NativeValidationRequestTests
{
    [Fact]
    public void Create_IsDeterministicAndResearchOnly()
    {
        var left = NativeValidationRequestRules.Create("SOURCE-A", "Ready");
        var right = NativeValidationRequestRules.Create("SOURCE-A", "Ready");
        Assert.Equal(left.Fingerprint, right.Fingerprint);
        Assert.False(left.CanSubmitOrders);
        NativeValidationRequestRules.Validate(left);
    }

    [Fact]
    public void Validate_RejectsOrderAuthority()
    {
        var value = NativeValidationRequestRules.Create("SOURCE-A", "Ready") with { CanSubmitOrders = true };
        Assert.Throws<InvalidOperationException>(() => NativeValidationRequestRules.Validate(value));
    }
}
