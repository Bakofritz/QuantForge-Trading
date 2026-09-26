using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ResearchResumeCheckpointFileStoreTests
{[Fact] public void Checkpoint_round_trips(){var v=ResearchResumeCheckpointRules.Create("w",ResearchResumeStage.EvidenceReady);var p=Path.GetTempFileName();try{var s=new ResearchResumeCheckpointFileStore(p);s.Save(v);Assert.Equal(v,s.Load());}finally{File.Delete(p);}}}
