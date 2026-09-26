using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ResearchWorkspaceAlertBundleTests
{[Fact] public void Bundle_has_workspace_identity(){var g=TestFixtures.Gates();var i=ResearchWorkspaceIndexRules.Create(new[]{ResearchWorkspaceIndexRules.From(g)});var b=ResearchWorkspaceAlertBundleRules.Create(i);Assert.Equal(i.IndexFingerprint,b.WorkspaceFingerprint);}}
