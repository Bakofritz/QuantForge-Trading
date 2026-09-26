namespace QuantForge.Core;
public sealed record ResearchIdentityDiagnostic(bool Matches,IReadOnlyList<string> Differences);
public static class ResearchIdentityDiagnosticRules
{
 public static ResearchIdentityDiagnostic Compare(ResearchReadinessSnapshot readiness,ResearchLaunchIntent launch)
 {
  ArgumentNullException.ThrowIfNull(readiness);ArgumentNullException.ThrowIfNull(launch);var d=new List<string>();
  if(readiness.DatasetFingerprint!=launch.Job.Identity.DatasetFingerprint)d.Add("dataset-fingerprint");
  if(readiness.StrategyFingerprint!=launch.Job.Identity.StrategyFingerprint)d.Add("strategy-fingerprint");
  if(!readiness.Ready)d.Add("readiness-incomplete");
  return new(d.Count==0,d);
 }
}
