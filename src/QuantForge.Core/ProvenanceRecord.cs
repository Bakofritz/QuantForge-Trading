namespace QuantForge.Core;

public readonly record struct ProvenanceRecord(
    string ArtifactId,
    string SourceUri,
    string RetrievedAtUtc,
    string Sha256,
    string OriginalArtifactFingerprint,
    string SanitizedArtifactFingerprint,
    string ScrubReportId);

public static class ProvenanceRules
{
    public static void RequireComplete(ProvenanceRecord record)
    {
        var values = new[]
        {
            record.ArtifactId,
            record.SourceUri,
            record.RetrievedAtUtc,
            record.Sha256,
            record.OriginalArtifactFingerprint,
            record.SanitizedArtifactFingerprint,
            record.ScrubReportId
        };

        if (values.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException(
                "Immutable provenance requires complete artifact and scrub identities.");
    }
}
