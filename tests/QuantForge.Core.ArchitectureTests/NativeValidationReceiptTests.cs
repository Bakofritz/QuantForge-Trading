using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class NativeValidationReceiptTests
{
    [Fact]
    public void Create_IsDeterministicAndResearchOnly()
    {
        var left = NativeValidationReceiptRules.Create("SOURCE-A", "Ready");
        var right = NativeValidationReceiptRules.Create("SOURCE-A", "Ready");
        Assert.Equal(left.Fingerprint, right.Fingerprint);
        Assert.False(left.CanSubmitOrders);
        NativeValidationReceiptRules.Validate(left);
    }

    [Fact]
    public void Validate_RejectsOrderAuthority()
    {
        var value = NativeValidationReceiptRules.Create("SOURCE-A", "Ready") with { CanSubmitOrders = true };
        Assert.Throws<InvalidOperationException>(() => NativeValidationReceiptRules.Validate(value));
    }
}
