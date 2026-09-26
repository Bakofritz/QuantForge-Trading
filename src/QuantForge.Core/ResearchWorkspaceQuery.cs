namespace QuantForge.Core;
public static class ResearchWorkspaceQuery
{
    public static IReadOnlyList<ResearchWorkspaceEntry> ByDataset(ResearchWorkspaceIndex index,string datasetFingerprint)
    {
        ResearchWorkspaceIndexRules.Validate(index);
        if(string.IsNullOrWhiteSpace(datasetFingerprint)) throw new ArgumentException("Dataset fingerprint is required.",nameof(datasetFingerprint));
        return index.Entries.Where(x=>string.Equals(x.DatasetFingerprint,datasetFingerprint,StringComparison.Ordinal)).ToArray();
    }
    public static IReadOnlyList<ResearchWorkspaceEntry> ByStrategy(ResearchWorkspaceIndex index,string strategyFingerprint)
    {
        ResearchWorkspaceIndexRules.Validate(index);
        if(string.IsNullOrWhiteSpace(strategyFingerprint)) throw new ArgumentException("Strategy fingerprint is required.",nameof(strategyFingerprint));
        return index.Entries.Where(x=>string.Equals(x.StrategyFingerprint,strategyFingerprint,StringComparison.Ordinal)).ToArray();
    }
    public static ResearchWorkspaceEntry? ByJob(ResearchWorkspaceIndex index,string jobFingerprint)
    {
        ResearchWorkspaceIndexRules.Validate(index);
        if(string.IsNullOrWhiteSpace(jobFingerprint)) throw new ArgumentException("Job fingerprint is required.",nameof(jobFingerprint));
        var matches=index.Entries.Where(x=>string.Equals(x.JobFingerprint,jobFingerprint,StringComparison.Ordinal)).ToArray();
        if(matches.Length>1) throw new InvalidOperationException("Workspace job identity is ambiguous.");
        return matches.SingleOrDefault();
    }
}
