namespace QuantForge.Core;

public sealed record DatasetResearchReadiness(
    string DatasetId,
    string DatasetFingerprint,
    bool CatalogAdmitted,
    bool AuthoritativeSessionCoverageStored,
    string? SessionCoverageArtifactFingerprint,
    bool ReadyForRemainingResearchGates,
    string Limitation);

public static class ResearchReadinessRules
{
    public static DatasetResearchReadiness Evaluate(DatasetCatalogEntry entry, SessionCoverageArtifact? coverageArtifact)
    {
        ArgumentNullException.ThrowIfNull(entry);
        DatasetCatalogRules.ValidateEntry(entry);

        if (coverageArtifact is null)
            return new(entry.DatasetId, entry.DatasetFingerprint, true, false, null, false,
                "Authoritative session coverage has not been persisted for this exact admitted dataset.");

        SessionCoverageArtifactRules.Validate(coverageArtifact);
        if (!string.Equals(coverageArtifact.Report.DatasetFingerprint, entry.DatasetFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Session coverage artifact does not belong to the admitted dataset fingerprint.");

        return new(entry.DatasetId, entry.DatasetFingerprint, true, true, coverageArtifact.ArtifactFingerprint, true,
            "Dataset identity and authoritative session coverage are ready; strategy admission, reliability, and workflow gates remain required.");
    }
}
