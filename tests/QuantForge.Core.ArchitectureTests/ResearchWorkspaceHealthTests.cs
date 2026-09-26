using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchWorkspaceHealthTests
{
    [Fact]
    public void Create_IsDeterministicAndResearchOnly()
    {
        var left = ResearchWorkspaceHealthRules.Create("SOURCE-A", "Ready");
        var right = ResearchWorkspaceHealthRules.Create("SOURCE-A", "Ready");
        Assert.Equal(left.Fingerprint, right.Fingerprint);
        Assert.False(left.CanSubmitOrders);
        ResearchWorkspaceHealthRules.Validate(left);
    }

    [Fact]
    public void Validate_RejectsOrderAuthority()
    {
        var value = ResearchWorkspaceHealthRules.Create("SOURCE-A", "Ready") with { CanSubmitOrders = true };
        Assert.Throws<InvalidOperationException>(() => ResearchWorkspaceHealthRules.Validate(value));
    }
}
