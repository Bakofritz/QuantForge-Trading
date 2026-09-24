using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public class AuthorityBoundaryTests
{
    [Fact]
    public void Research_domains_cannot_submit_orders_or_change_settings()
    {
        foreach (var domain in new[]
        {
            AuthorityDomain.HistoricalResearch,
            AuthorityDomain.SimulatedReplay,
            AuthorityDomain.SimulatedAccount,
            AuthorityDomain.ReadOnlyResearch
        })
        {
            var decision = AuthorityBoundary.EvaluateResearch(domain);
            Assert.False(decision.CanSubmitOrders);
            Assert.False(decision.CanChangeApplicationSettings);
        }
    }

    [Fact]
    public void Live_domain_is_rejected()
    {
        Assert.Throws<InvalidOperationException>(
            () => AuthorityBoundary.EvaluateResearch(AuthorityDomain.LiveAccount));
    }
}
