namespace QuantForge.Core;
public static class ResearchWorkspaceRebuilder
{
 public static ResearchWorkspaceIndex Rebuild(IEnumerable<ResearchWorkspaceEntry> discovered)
 {
  ArgumentNullException.ThrowIfNull(discovered);var entries=discovered.ToArray();if(entries.Length==0)throw new InvalidOperationException("Workspace rebuild requires discovered evidence.");return ResearchWorkspaceIndexRules.Create(entries);
 }
 public static ResearchWorkspaceIndex Merge(ResearchWorkspaceIndex current,IEnumerable<ResearchWorkspaceEntry> discovered)
 {
  ResearchWorkspaceIndexRules.Validate(current);ArgumentNullException.ThrowIfNull(discovered);
  var combined=current.Entries.Concat(discovered).GroupBy(x=>x.EntryType+"|"+x.Fingerprint,StringComparer.Ordinal).Select(x=>x.First()).ToArray();return ResearchWorkspaceIndexRules.Create(combined);
 }
}
