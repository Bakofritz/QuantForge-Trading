using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ResearchActionAvailabilityFileStoreTests
{[Fact] public void Live_authority_is_rejected(){var p=Path.GetTempFileName();try{var s=new ResearchActionAvailabilityFileStore(p);Assert.Throws<InvalidOperationException>(()=>s.Save(new(true,false,false,true,true,false)));}finally{File.Delete(p);}}}
