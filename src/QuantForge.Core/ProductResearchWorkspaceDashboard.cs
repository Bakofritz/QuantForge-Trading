namespace QuantForge.Core;
public sealed record ProductResearchWorkspaceDashboard(string WorkspaceFingerprint,bool Consistent,int Datasets,int Gates,int Launches,int Outcomes,int Comparisons,bool CanLaunchResearch,bool LiveAccountEnabled,bool CanSubmitOrders,IReadOnlyList<string> Diagnostics);
public static class ProductResearchWorkspaceDashboardRules
{
 public static ProductResearchWorkspaceDashboard Create(ResearchWorkspaceIndex index)
 {
  var consistency=ResearchWorkspaceConsistencyRules.Validate(index);var datasets=index.Entries.Select(x=>x.DatasetFingerprint).Distinct(StringComparer.Ordinal).Count();
  return new(index.IndexFingerprint,consistency.Valid,datasets,consistency.GateEntries,consistency.LaunchEntries,consistency.OutcomeEntries,consistency.ComparisonEntries,consistency.Valid&&consistency.GateEntries>0,false,false,consistency.Diagnostics);
 }
}
