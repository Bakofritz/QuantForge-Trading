using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class AndroidResearchHomeTests
{[Fact] public void Android_home_is_research_only(){var g=TestFixtures.Gates();var h=AndroidResearchHomeRules.Create(ResearchWorkspaceIndexRules.Create(new[]{ResearchWorkspaceIndexRules.From(g)}));Assert.True(h.CanPrepareResearch);Assert.False(h.LiveTradingEnabled);Assert.False(h.OrderSubmissionEnabled);}}
