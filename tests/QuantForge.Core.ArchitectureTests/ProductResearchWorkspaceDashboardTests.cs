using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ProductResearchWorkspaceDashboardTests
{[Fact] public void Dashboard_never_enables_live_or_order_authority(){var g=TestFixtures.Gates();var i=ResearchWorkspaceIndexRules.Create(new[]{ResearchWorkspaceIndexRules.From(g)});var d=ProductResearchWorkspaceDashboardRules.Create(i);Assert.True(d.CanLaunchResearch);Assert.False(d.LiveAccountEnabled);Assert.False(d.CanSubmitOrders);}}
