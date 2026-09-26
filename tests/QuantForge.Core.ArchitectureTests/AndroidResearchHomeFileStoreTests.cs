using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class AndroidResearchHomeFileStoreTests
{[Fact] public void Android_home_round_trips(){var g=TestFixtures.Gates();var v=AndroidResearchHomeRules.Create(ResearchWorkspaceIndexRules.Create(new[]{ResearchWorkspaceIndexRules.From(g)}));var p=Path.GetTempFileName();try{var s=new AndroidResearchHomeFileStore(p);s.Save(v);Assert.Equal(v,s.Load());}finally{File.Delete(p);}}}
