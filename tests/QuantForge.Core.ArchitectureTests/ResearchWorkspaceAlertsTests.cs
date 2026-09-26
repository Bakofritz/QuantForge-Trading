using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ResearchWorkspaceAlertsTests
{[Fact] public void Gate_only_workspace_reports_no_outcomes(){var g=TestFixtures.Gates();var i=ResearchWorkspaceIndexRules.Create(new[]{ResearchWorkspaceIndexRules.From(g)});Assert.Contains(ResearchWorkspaceAlertRules.Create(i),x=>x.Code=="no-outcomes");}}
