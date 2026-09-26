using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class AndroidResearchExportBundleTests
{
    [Fact]
    public void Create_IsDeterministicAndResearchOnly()
    {
        var left = AndroidResearchExportBundleRules.Create("SOURCE-A", "Ready");
        var right = AndroidResearchExportBundleRules.Create("SOURCE-A", "Ready");
        Assert.Equal(left.Fingerprint, right.Fingerprint);
        Assert.False(left.CanSubmitOrders);
        AndroidResearchExportBundleRules.Validate(left);
    }

    [Fact]
    public void Validate_RejectsOrderAuthority()
    {
        var value = AndroidResearchExportBundleRules.Create("SOURCE-A", "Ready") with { CanSubmitOrders = true };
        Assert.Throws<InvalidOperationException>(() => AndroidResearchExportBundleRules.Validate(value));
    }
}
