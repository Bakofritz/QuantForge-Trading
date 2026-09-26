namespace QuantForge.Core;
public sealed record ResearchWorkspaceConsistency(bool Valid,int GateEntries,int LaunchEntries,int OutcomeEntries,int ComparisonEntries,IReadOnlyList<string> Diagnostics);
public static class ResearchWorkspaceConsistencyRules
{
 public static ResearchWorkspaceConsistency Validate(ResearchWorkspaceIndex index)
 {
  ResearchWorkspaceIndexRules.Validate(index);var d=new List<string>();
  var gates=index.Entries.Where(x=>x.EntryType=="research-gates").ToArray();var launches=index.Entries.Where(x=>x.EntryType=="research-launch").ToArray();var outcomes=index.Entries.Where(x=>x.EntryType=="research-outcome").ToArray();var comparisons=index.Entries.Where(x=>x.EntryType=="research-comparison").ToArray();
  foreach(var launch in launches)if(!gates.Any(g=>g.DatasetFingerprint==launch.DatasetFingerprint&&g.StrategyFingerprint==launch.StrategyFingerprint))d.Add("launch-without-matching-gates:"+launch.Fingerprint);
  foreach(var outcome in outcomes)if(!launches.Any(l=>l.DatasetFingerprint==outcome.DatasetFingerprint&&l.StrategyFingerprint==outcome.StrategyFingerprint))d.Add("outcome-without-matching-launch:"+outcome.Fingerprint);
  return new(d.Count==0,gates.Length,launches.Length,outcomes.Length,comparisons.Length,d);
 }
}
