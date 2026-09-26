using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ResearchWorkspaceActivityTests
{[Fact] public void Activity_is_deterministically_ordered(){var g=TestFixtures.Gates();var a=ResearchWorkspaceActivityRules.Create(ResearchWorkspaceIndexRules.Create(new[]{ResearchWorkspaceIndexRules.From(g)}));Assert.Single(a);Assert.Equal("research-gates",a[0].EntryType);}}
