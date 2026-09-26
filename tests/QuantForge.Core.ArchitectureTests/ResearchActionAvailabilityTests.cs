using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ResearchActionAvailabilityTests
{[Fact] public void Research_actions_never_grant_live_authority(){var g=TestFixtures.Gates();var d=ProductResearchWorkspaceDashboardRules.Create(ResearchWorkspaceIndexRules.Create(new[]{ResearchWorkspaceIndexRules.From(g)}));var a=ResearchActionAvailabilityRules.Create(d);Assert.True(a.CanPrepareLaunch);Assert.False(a.CanSubmitOrders);Assert.False(a.CanEnableLiveAccount);}}
