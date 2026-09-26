using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ActiveStrategySelectionFileStoreTests
{[Fact] public void Selection_round_trips(){var v=ActiveStrategySelectionRules.Create(TestFixtures.Gates().Strategy);var p=Path.GetTempFileName();try{var s=new ActiveStrategySelectionFileStore(p);s.Save(v);Assert.Equal(v,s.Load());}finally{File.Delete(p);}}}
