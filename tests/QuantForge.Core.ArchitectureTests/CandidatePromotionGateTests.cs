using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class CandidatePromotionGateTests
{
    [Fact]
    public void Create_IsDeterministicAndResearchOnly()
    {
        var left = CandidatePromotionGateRules.Create("SOURCE-A", "Ready");
        var right = CandidatePromotionGateRules.Create("SOURCE-A", "Ready");
        Assert.Equal(left.Fingerprint, right.Fingerprint);
        Assert.False(left.CanSubmitOrders);
        CandidatePromotionGateRules.Validate(left);
    }

    [Fact]
    public void Validate_RejectsOrderAuthority()
    {
        var value = CandidatePromotionGateRules.Create("SOURCE-A", "Ready") with { CanSubmitOrders = true };
        Assert.Throws<InvalidOperationException>(() => CandidatePromotionGateRules.Validate(value));
    }
}
