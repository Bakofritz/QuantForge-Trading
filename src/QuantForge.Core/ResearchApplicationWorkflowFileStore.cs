namespace QuantForge.Core;

public sealed class ResearchApplicationWorkflowFileStore
{
    private readonly ResearchWorkflowSummaryFileStore _summaries;
    private readonly ResearchResultArchiveFileStore _results;

    public ResearchApplicationWorkflowFileStore(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("A workflow application store directory is required.", nameof(rootDirectory));
        var root = Path.GetFullPath(rootDirectory);
        _summaries = new ResearchWorkflowSummaryFileStore(Path.Combine(root, "workflows"));
        _results = new ResearchResultArchiveFileStore(Path.Combine(root, "results"));
    }

    public void Save(ResearchApplicationWorkflowResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        var summaryPath = _summaries.Save(result.Summary);
        try
        {
            foreach (var publication in result.Publications)
                _results.Save(publication);
        }
        catch
        {
            // Existing validated workflow summaries are immutable. A failed result publication
            // is not converted into a false success; callers receive the exception.
            if (File.Exists(summaryPath) && result.Publications.Count > 0)
            {
                // Keep the summary for forensic recovery; no rollback can erase an already
                // published immutable workflow without violating provenance history.
            }
            throw;
        }
    }
}
