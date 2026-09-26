using System.Security.Cryptography;
using System.Text;
namespace QuantForge.Core;
public enum ResearchResumeStage{EvidenceReady,LaunchPrepared,TerminalOutcomeRecorded,ReviewReady}
public sealed record ResearchResumeCheckpoint(string CheckpointFingerprint,string WorkspaceFingerprint,ResearchResumeStage Stage,string? LaunchFingerprint,string? OutcomeFingerprint);
public static class ResearchResumeCheckpointRules
{
 public static ResearchResumeCheckpoint Create(string workspaceFingerprint,ResearchResumeStage stage,string? launchFingerprint=null,string? outcomeFingerprint=null)
 {
  if(string.IsNullOrWhiteSpace(workspaceFingerprint))throw new InvalidOperationException("Resume checkpoint requires workspace identity.");
  if(stage>=ResearchResumeStage.LaunchPrepared&&string.IsNullOrWhiteSpace(launchFingerprint))throw new InvalidOperationException("Launch-stage checkpoint requires launch identity.");
  if(stage>=ResearchResumeStage.TerminalOutcomeRecorded&&string.IsNullOrWhiteSpace(outcomeFingerprint))throw new InvalidOperationException("Terminal checkpoint requires outcome identity.");
  var text=string.Join("|",workspaceFingerprint,stage,launchFingerprint??"",outcomeFingerprint??"");var fp=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));return new(fp,workspaceFingerprint,stage,launchFingerprint,outcomeFingerprint);
 }
}
