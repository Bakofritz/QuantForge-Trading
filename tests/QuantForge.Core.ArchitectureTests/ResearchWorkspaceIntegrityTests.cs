using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ResearchWorkspaceIntegrityTests
{
    [Fact] public void Canonical_workspace_is_consistent()
    {
        var gates=TestFixtures.Gates();
        var index=ResearchWorkspaceIndexRules.Create(new[]{ResearchWorkspaceIndexRules.From(gates)});
        Assert.True(ResearchWorkspaceIntegrityRules.Inspect(index).IsConsistent);
    }
}
