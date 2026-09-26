using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ResearchWorkspaceQueryTests
{
 [Fact] public void Dataset_query_is_exact(){var g=TestFixtures.Gates();var i=ResearchWorkspaceIndexRules.Create(new[]{ResearchWorkspaceIndexRules.From(g)});Assert.Single(ResearchWorkspaceQuery.ByDataset(i,g.Dataset.DatasetFingerprint));}
}
