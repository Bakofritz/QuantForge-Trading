namespace QuantForge.Core;
public sealed record ResearchWorkspaceActivityItem(string EntryType,string Fingerprint,string DatasetFingerprint,string? StrategyFingerprint,string? JobFingerprint);
public static class ResearchWorkspaceActivityRules
{
 public static IReadOnlyList<ResearchWorkspaceActivityItem> Create(ResearchWorkspaceIndex index){ResearchWorkspaceIndexRules.Validate(index);return index.Entries.OrderBy(x=>x.EntryType,StringComparer.Ordinal).ThenBy(x=>x.Fingerprint,StringComparer.Ordinal).Select(x=>new ResearchWorkspaceActivityItem(x.EntryType,x.Fingerprint,x.DatasetFingerprint,x.StrategyFingerprint,x.JobFingerprint)).ToArray();}
}
