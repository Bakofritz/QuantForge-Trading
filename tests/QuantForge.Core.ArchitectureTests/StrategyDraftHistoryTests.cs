using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class StrategyDraftHistoryTests
{
    [Fact]
    public void Create_IsDeterministicAndResearchOnly()
    {
        var left = StrategyDraftHistoryRules.Create("SOURCE-A", "Ready");
        var right = StrategyDraftHistoryRules.Create("SOURCE-A", "Ready");
        Assert.Equal(left.Fingerprint, right.Fingerprint);
        Assert.False(left.CanSubmitOrders);
        StrategyDraftHistoryRules.Validate(left);
    }

    [Fact]
    public void Validate_RejectsOrderAuthority()
    {
        var value = StrategyDraftHistoryRules.Create("SOURCE-A", "Ready") with { CanSubmitOrders = true };
        Assert.Throws<InvalidOperationException>(() => StrategyDraftHistoryRules.Validate(value));
    }
}
