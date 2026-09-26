using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core;

public sealed record ResearchPublicationArtifact(
    ResearchPublicationBundle Publication,
    string ChartData,
    string LedgerData,
    string ArtifactFingerprint,
    string Manifest);

public static class ResearchPublicationArtifactRules
{
    public static void Validate(ResearchPublicationArtifact artifact)
    {
        ArgumentNullException.ThrowIfNull(artifact);
        ResearchReportRules.Validate(artifact.Publication.Report);
        ResearchExecutionTraceRules.Validate(artifact.Publication.Report.ExecutionTrace!);
        if (artifact.Publication.Report.Status != ResearchResultStatus.Complete)
            throw new InvalidOperationException("Publication artifact must contain a complete report.");
        if (string.IsNullOrWhiteSpace(artifact.ChartData) || string.IsNullOrWhiteSpace(artifact.LedgerData) ||
            string.IsNullOrWhiteSpace(artifact.Manifest) || string.IsNullOrWhiteSpace(artifact.ArtifactFingerprint))
            throw new InvalidOperationException("Publication artifact is incomplete.");
        if (!artifact.Manifest.Contains($"trace={artifact.Publication.Report.ExecutionTrace!.Value.TraceFingerprint}", StringComparison.Ordinal))
            throw new InvalidOperationException("Publication artifact manifest does not bind the execution trace.");
    }
}

public static class ResearchPublicationArtifactFactory
{
    public static ResearchPublicationArtifact Create(
        ResearchPublicationBundle publication,
        ResearchResultPipelineOutput output)
    {
        ArgumentNullException.ThrowIfNull(publication);
        ArgumentNullException.ThrowIfNull(output);

        if (output.Report.Status != ResearchResultStatus.Complete || output.Execution is null ||
            output.ChartData is null || output.LedgerData is null)
            throw new InvalidOperationException("Publication artifacts require a complete execution result.");

        ResearchReportRules.Validate(publication.Report);
        ResearchExecutionTraceRules.Validate(output.Execution);

        if (!string.Equals(output.Report.JobId, publication.Provenance.JobFingerprint, StringComparison.Ordinal) ||
            !string.Equals(output.Report.DatasetFingerprint, publication.Provenance.DatasetFingerprint, StringComparison.Ordinal) ||
            !string.Equals(output.Report.EvidenceTail?.Fingerprint, publication.Provenance.EvidenceFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Publication artifact identities do not match the bound provenance.");

        var manifest = string.Join("\n", new[]
        {
            "quantforge-publication-v1",
            $"job={publication.Provenance.JobFingerprint}",
            $"dataset={publication.Provenance.DatasetFingerprint}",
            $"evidence={publication.Provenance.EvidenceFingerprint}",
            $"trace={output.Execution.TraceFingerprint}",
            $"chart-sha256={Sha(output.ChartData)}",
            $"ledger-sha256={Sha(output.LedgerData)}"
        });

        var fingerprint = Sha(manifest);
        var artifact = new ResearchPublicationArtifact(publication, output.ChartData, output.LedgerData, fingerprint, manifest);
        ResearchPublicationArtifactRules.Validate(artifact);
        return artifact;
    }

    public static ResearchPublicationArtifact Create(
        ResearchPublicationEvidence evidence,
        ResearchResultPipelineOutput output)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        ResearchPublicationEvidenceRules.Validate(evidence);
        var artifact = Create(evidence.Publication, output);
        var manifest = artifact.Manifest + "\n" +
            $"session-policy={evidence.SessionCoverage.PolicyFingerprint}";
        var fingerprint = Sha(manifest);
        var enriched = artifact with { ArtifactFingerprint = fingerprint, Manifest = manifest };
        ResearchPublicationArtifactRules.Validate(enriched);
        return enriched;
    }

    private static string Sha(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
