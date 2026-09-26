namespace QuantForge.Core;

public sealed record ResearchWorkspaceIntegrityReport(
    bool IsConsistent,
    int EntryCount,
    int DatasetCount,
    IReadOnlyList<string> Diagnostics);

public static class ResearchWorkspaceIntegrityRules
{
    public static ResearchWorkspaceIntegrityReport Inspect(ResearchWorkspaceIndex index)
    {
        ResearchWorkspaceIndexRules.Validate(index);
        var diagnostics = new List<string>();
        foreach (var group in index.Entries.GroupBy(x => x.JobFingerprint, StringComparer.Ordinal))
        {
            if (group.Key is null) continue;
            if (group.Select(x => x.DatasetFingerprint).Distinct(StringComparer.Ordinal).Count() > 1)
                diagnostics.Add($"Job {group.Key} references multiple datasets.");
            if (group.Select(x => x.StrategyFingerprint).Where(x => x is not null).Distinct(StringComparer.Ordinal).Count() > 1)
                diagnostics.Add($"Job {group.Key} references multiple strategies.");
        }
        return new(diagnostics.Count == 0, index.Entries.Count,
            index.Entries.Select(x => x.DatasetFingerprint).Distinct(StringComparer.Ordinal).Count(), diagnostics);
    }
}
