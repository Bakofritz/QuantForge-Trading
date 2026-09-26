namespace QuantForge.Core;

public sealed record ResearchApplicationRecoveryResult(
    ResearchWorkflowSummary Summary,
    ProductUiWorkflowState UiState);

public static class ResearchApplicationRecovery
{
    public static ResearchApplicationRecoveryResult Load(
        string rootDirectory,
        string workflowFingerprint,
        IEnumerable<string> artifactFingerprints)
    {
        ArgumentNullException.ThrowIfNull(artifactFingerprints);
        var root = Path.GetFullPath(rootDirectory);
        var summaries = new ResearchWorkflowSummaryFileStore(Path.Combine(root, "workflows"));
        var results = new ResearchResultArchiveFileStore(Path.Combine(root, "results"));
        var summary = summaries.Load(workflowFingerprint);

        foreach (var fingerprint in artifactFingerprints)
            results.VerifyStored(fingerprint);

        return new ResearchApplicationRecoveryResult(
            summary,
            ProductUiBoundary.CreateReadOnlyState(summary));
    }
}
