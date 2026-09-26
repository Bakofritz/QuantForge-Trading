using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ResearchWorkspaceActivityFileStoreTests
{[Fact] public void Activity_round_trips(){var g=TestFixtures.Gates();var v=ResearchWorkspaceActivityRules.Create(ResearchWorkspaceIndexRules.Create(new[]{ResearchWorkspaceIndexRules.From(g)}));var p=Path.GetTempFileName();try{var s=new ResearchWorkspaceActivityFileStore(p);s.Save(v);Assert.Single(s.Load());}finally{File.Delete(p);}}}
