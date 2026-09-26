using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core;

public sealed record ResearchWorkflowPackage(
    string PackageFingerprint,
    ResearchWorkflowSummary Summary,
    IReadOnlyList<ResearchPublicationArtifact> Publications,
    string Manifest,
    string SummaryMarkdown);

public static class ResearchWorkflowPackageFactory
{
    public static ResearchWorkflowPackage Create(
        ResearchWorkflowSummary summary,
        IReadOnlyList<ResearchPublicationArtifact> publications)
    {
        ArgumentNullException.ThrowIfNull(summary);
        ArgumentNullException.ThrowIfNull(publications);
        if (publications.Count == 0)
            throw new InvalidOperationException("A research workflow package requires at least one publication.");

        var ordered = publications.OrderBy(x => x.ArtifactFingerprint, StringComparer.Ordinal).ToArray();
        foreach (var publication in ordered)
            ResearchPublicationArtifactRules.Validate(publication);

        foreach (var publication in ordered)
        {
            if (!summary.Reports.Any(x => string.Equals(x.JobId, publication.Provenance.JobFingerprint, StringComparison.Ordinal)))
                throw new InvalidOperationException("Every publication must correspond to a workflow report.");
        }

        var manifest = string.Join("\n", new[]
        {
            "quantforge-workflow-package-v1",
            $"workflow={summary.WorkflowFingerprint}",
            $"mode={summary.Mode}",
            $"runs={summary.TotalRuns}",
            $"complete={summary.CompleteRuns}",
            $"blocked={summary.DataBlockedRuns}",
            $"invalid={summary.InvalidRuns}",
            $"publications={string.Join(",", ordered.Select(x => x.ArtifactFingerprint))}"
        });

        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(manifest)));
        return new ResearchWorkflowPackage(
            fingerprint,
            summary,
            ordered,
            manifest,
            ResearchWorkflowMarkdownExporter.Export(summary));
    }
}

public static class ResearchWorkflowPackageRules
{
    public static void Validate(ResearchWorkflowPackage package)
    {
        ArgumentNullException.ThrowIfNull(package);
        if (string.IsNullOrWhiteSpace(package.PackageFingerprint) ||
            string.IsNullOrWhiteSpace(package.Manifest) ||
            string.IsNullOrWhiteSpace(package.SummaryMarkdown) ||
            package.Publications is null || package.Publications.Count == 0)
            throw new InvalidOperationException("Research workflow package is incomplete.");

        ResearchWorkflowSummaryFactory.Create(package.Summary.Mode, new ResearchWorkflowResult(
            package.Summary.Reports,
            package.Summary.Reports.Select(ResearchComponentStatusRules.FromReport).ToArray(),
            package.Summary.Reliability));

        foreach (var publication in package.Publications)
            ResearchPublicationArtifactRules.Validate(publication);
    }
}
