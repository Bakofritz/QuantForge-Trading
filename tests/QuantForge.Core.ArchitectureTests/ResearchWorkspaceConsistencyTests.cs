using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ResearchWorkspaceConsistencyTests
{[Fact] public void Gate_only_workspace_is_consistent(){var g=TestFixtures.Gates();var i=ResearchWorkspaceIndexRules.Create(new[]{ResearchWorkspaceIndexRules.From(g)});Assert.True(ResearchWorkspaceConsistencyRules.Validate(i).Valid);}}
