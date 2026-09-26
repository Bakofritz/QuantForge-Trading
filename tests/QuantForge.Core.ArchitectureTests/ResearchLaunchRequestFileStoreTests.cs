using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ResearchLaunchRequestFileStoreTests
{[Fact] public void Launch_request_round_trips(){var g=TestFixtures.Gates();var v=ResearchLaunchRequestRules.Create(ResearchReadinessSnapshotRules.Create(g),ResearchLaunchIntentRules.Create(g,TestFixtures.Job(g)));var p=Path.GetTempFileName();try{var s=new ResearchLaunchRequestFileStore(p);s.Save(v);Assert.Equal(v.RequestFingerprint,s.Load().RequestFingerprint);}finally{File.Delete(p);}}}
