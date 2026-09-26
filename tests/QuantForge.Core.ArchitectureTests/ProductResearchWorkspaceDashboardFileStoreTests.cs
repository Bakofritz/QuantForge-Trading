using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ProductResearchWorkspaceDashboardFileStoreTests
{[Fact] public void Dashboard_round_trips_without_live_authority(){var g=TestFixtures.Gates();var i=ResearchWorkspaceIndexRules.Create(new[]{ResearchWorkspaceIndexRules.From(g)});var v=ProductResearchWorkspaceDashboardRules.Create(i);var p=Path.GetTempFileName();try{var s=new ProductResearchWorkspaceDashboardFileStore(p);s.Save(v);Assert.Equal(v,s.Load());}finally{File.Delete(p);}}}
