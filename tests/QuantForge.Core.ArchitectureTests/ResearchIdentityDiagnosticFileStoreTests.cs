using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ResearchIdentityDiagnosticFileStoreTests
{[Fact] public void Diagnostic_round_trips(){var p=Path.GetTempFileName();try{var v=new ResearchIdentityDiagnostic(false,new[]{"dataset-fingerprint"});var s=new ResearchIdentityDiagnosticFileStore(p);s.Save(v);Assert.Equal(v.Differences,s.Load().Differences);}finally{File.Delete(p);}}}
