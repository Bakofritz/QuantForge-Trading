using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ResearchReadinessSnapshotFileStoreTests
{[Fact] public void Ready_snapshot_round_trips(){var p=Path.GetTempFileName();try{var s=ResearchReadinessSnapshotRules.Create(TestFixtures.Gates());var store=new ResearchReadinessSnapshotFileStore(p);store.Save(s);Assert.Equal(s,store.Load());}finally{File.Delete(p);}}}
