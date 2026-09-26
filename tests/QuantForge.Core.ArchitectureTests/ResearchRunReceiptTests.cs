using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchRunReceiptTests
{
    [Fact]
    public void Create_IsDeterministicAndResearchOnly()
    {
        var left = ResearchRunReceiptRules.Create("SOURCE-A", "Ready");
        var right = ResearchRunReceiptRules.Create("SOURCE-A", "Ready");
        Assert.Equal(left.Fingerprint, right.Fingerprint);
        Assert.False(left.CanSubmitOrders);
        ResearchRunReceiptRules.Validate(left);
    }

    [Fact]
    public void Validate_RejectsOrderAuthority()
    {
        var value = ResearchRunReceiptRules.Create("SOURCE-A", "Ready") with { CanSubmitOrders = true };
        Assert.Throws<InvalidOperationException>(() => ResearchRunReceiptRules.Validate(value));
    }
}
