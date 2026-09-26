using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class NativeBuildEvidenceTests
{[Fact] public void Passing_build_cannot_have_errors(){Assert.Throws<InvalidOperationException>(()=>NativeBuildEvidenceRules.Create("android","c","t","1","1","app.apk","ABC",1,0,1,true,false));}[Fact] public void Evidence_is_deterministic(){var a=NativeBuildEvidenceRules.Create("android","c","t","1","1","app.apk","ABC",1,0,0,true,false);var b=NativeBuildEvidenceRules.Create("android","c","t","1","1","app.apk","ABC",1,0,0,true,false);Assert.Equal(a,b);}}
