using System.Security.Cryptography;
using System.Text;
namespace QuantForge.Core;
public sealed record ActiveStrategySelection(string SelectionFingerprint,string StrategyArtifactFingerprint,string StrategyId,string StrategyFingerprint);
public static class ActiveStrategySelectionRules
{
 public static ActiveStrategySelection Create(StrategyAdmissionArtifact artifact)
 {
  StrategyAdmissionArtifactRules.Validate(artifact);
  var m=artifact.Envelope.Manifest;var payload=artifact.ArtifactFingerprint+"|"+m.StrategyId+"|"+m.SourceFingerprint;
  var fp=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
  return new(fp,artifact.ArtifactFingerprint,m.StrategyId,m.SourceFingerprint);
 }
 public static void Validate(ActiveStrategySelection selection,StrategyAdmissionArtifact artifact){if(Create(artifact)!=selection)throw new InvalidOperationException("Active strategy selection does not match admitted strategy evidence.");}
}
