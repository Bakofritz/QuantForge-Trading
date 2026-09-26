using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchOutcomeDigestTests
{
    [Fact]
    public void Create_IsDeterministicAndResearchOnly()
    {
        var left = ResearchOutcomeDigestRules.Create("SOURCE-A", "Ready");
        var right = ResearchOutcomeDigestRules.Create("SOURCE-A", "Ready");
        Assert.Equal(left.Fingerprint, right.Fingerprint);
        Assert.False(left.CanSubmitOrders);
        ResearchOutcomeDigestRules.Validate(left);
    }

    [Fact]
    public void Validate_RejectsOrderAuthority()
    {
        var value = ResearchOutcomeDigestRules.Create("SOURCE-A", "Ready") with { CanSubmitOrders = true };
        Assert.Throws<InvalidOperationException>(() => ResearchOutcomeDigestRules.Validate(value));
    }
}
