using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchComparisonDigestTests
{
    [Fact]
    public void Create_IsDeterministicAndResearchOnly()
    {
        var left = ResearchComparisonDigestRules.Create("SOURCE-A", "Ready");
        var right = ResearchComparisonDigestRules.Create("SOURCE-A", "Ready");
        Assert.Equal(left.Fingerprint, right.Fingerprint);
        Assert.False(left.CanSubmitOrders);
        ResearchComparisonDigestRules.Validate(left);
    }

    [Fact]
    public void Validate_RejectsOrderAuthority()
    {
        var value = ResearchComparisonDigestRules.Create("SOURCE-A", "Ready") with { CanSubmitOrders = true };
        Assert.Throws<InvalidOperationException>(() => ResearchComparisonDigestRules.Validate(value));
    }
}
