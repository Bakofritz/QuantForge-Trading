namespace QuantForge.Core;

public enum MarketAdmissionReadinessStatus
{
    Blocked,
    NeedsEvidence,
    Ready
}

public enum MarketAdmissionRequirement
{
    CompletedInspection,
    AdmissionEligibleCandidate,
    ExternalDatasetId,
    ExternalInstrumentIdentity,
    ExternalTimeframeIdentity,
    ImmutableProvenance,
    FingerprintAgreement,
    ResolveIdentityConflict,
    ResolveDescriptiveLabelConflict
}

public sealed record MarketAdmissionReadiness(
    int FileIndex,
    MarketAdmissionReadinessStatus Status,
    string? DatasetFingerprint,
    string InspectionCode,
    IReadOnlyList<MarketAdmissionRequirement> Requirements)
{
    public bool CanCreateAdmissionRequest => Status == MarketAdmissionReadinessStatus.Ready;
}

/// <summary>
/// Explains what is still required before a structurally inspected batch file can enter the
/// existing immutable data-admission pipeline. This evaluator never infers missing identity.
/// </summary>
public static class MarketAdmissionReadinessRules
{
    public static MarketAdmissionReadiness Assess(
        MarketBatchFile file,
        string? datasetId = null,
        string? instrument = null,
        string? timeframe = null,
        ProvenanceRecord? provenance = null)
    {
        ArgumentNullException.ThrowIfNull(file);
        var requirements = new List<MarketAdmissionRequirement>();
        var fingerprint = file.Data?.Hash;

        if (file.Data?.Complete != true || string.IsNullOrWhiteSpace(fingerprint) || file.Data.Rows is not { Count: > 0 })
            requirements.Add(MarketAdmissionRequirement.CompletedInspection);

        switch (file.State)
        {
            case MarketFileState.Inspected:
                break;
            case MarketFileState.IdentityConflict:
                requirements.Add(MarketAdmissionRequirement.ResolveIdentityConflict);
                requirements.Add(MarketAdmissionRequirement.AdmissionEligibleCandidate);
                break;
            case MarketFileState.UnresolvedIdentity:
            case MarketFileState.Duplicate:
            case MarketFileState.Rejected:
                requirements.Add(MarketAdmissionRequirement.AdmissionEligibleCandidate);
                break;
            default:
                requirements.Add(MarketAdmissionRequirement.AdmissionEligibleCandidate);
                break;
        }

        if (string.IsNullOrWhiteSpace(datasetId)) requirements.Add(MarketAdmissionRequirement.ExternalDatasetId);
        if (string.IsNullOrWhiteSpace(instrument)) requirements.Add(MarketAdmissionRequirement.ExternalInstrumentIdentity);
        if (string.IsNullOrWhiteSpace(timeframe)) requirements.Add(MarketAdmissionRequirement.ExternalTimeframeIdentity);

        if (provenance is null)
        {
            requirements.Add(MarketAdmissionRequirement.ImmutableProvenance);
        }
        else
        {
            try
            {
                ProvenanceRules.RequireComplete(provenance.Value);
                if (!string.Equals(fingerprint, provenance.Value.SanitizedArtifactFingerprint, StringComparison.Ordinal))
                    requirements.Add(MarketAdmissionRequirement.FingerprintAgreement);
            }
            catch (InvalidOperationException)
            {
                requirements.Add(MarketAdmissionRequirement.ImmutableProvenance);
            }
        }

        if (!string.IsNullOrWhiteSpace(instrument) && file.Label is not null &&
            !string.Equals(file.Label.Instrument, instrument, StringComparison.OrdinalIgnoreCase))
            requirements.Add(MarketAdmissionRequirement.ResolveDescriptiveLabelConflict);

        var distinct = requirements.Distinct().OrderBy(x => x).ToArray();
        if (distinct.Length == 0)
        {
            // Reuse the authoritative admission conversion as the final dry-run check.
            _ = MarketBatchAdmissionPipeline.CreateCatalogEntry(new(
                datasetId!, instrument!, timeframe!, provenance!.Value, file));
            return new(file.Index, MarketAdmissionReadinessStatus.Ready, fingerprint, file.Code, distinct);
        }

        var blocked = distinct.Any(x => x is
            MarketAdmissionRequirement.CompletedInspection or
            MarketAdmissionRequirement.AdmissionEligibleCandidate or
            MarketAdmissionRequirement.FingerprintAgreement or
            MarketAdmissionRequirement.ResolveIdentityConflict or
            MarketAdmissionRequirement.ResolveDescriptiveLabelConflict);
        return new(file.Index, blocked ? MarketAdmissionReadinessStatus.Blocked : MarketAdmissionReadinessStatus.NeedsEvidence,
            fingerprint, file.Code, distinct);
    }

    public static IReadOnlyList<MarketAdmissionReadiness> AssessInspectionBatch(MarketBatchResult batch)
    {
        ArgumentNullException.ThrowIfNull(batch);
        if (!batch.Completed) return Array.Empty<MarketAdmissionReadiness>();
        return batch.Files.OrderBy(file => file.Index).Select(file => Assess(file)).ToArray();
    }

    public static string Describe(MarketAdmissionRequirement requirement) => requirement switch
    {
        MarketAdmissionRequirement.CompletedInspection => "complete structural inspection",
        MarketAdmissionRequirement.AdmissionEligibleCandidate => "an admission-eligible inspected source",
        MarketAdmissionRequirement.ExternalDatasetId => "external dataset ID",
        MarketAdmissionRequirement.ExternalInstrumentIdentity => "independent instrument identity",
        MarketAdmissionRequirement.ExternalTimeframeIdentity => "independent timeframe identity",
        MarketAdmissionRequirement.ImmutableProvenance => "immutable provenance",
        MarketAdmissionRequirement.FingerprintAgreement => "provenance/inspected-byte fingerprint agreement",
        MarketAdmissionRequirement.ResolveIdentityConflict => "identity-conflict resolution",
        MarketAdmissionRequirement.ResolveDescriptiveLabelConflict => "external identity/filename-label conflict resolution",
        _ => throw new ArgumentOutOfRangeException(nameof(requirement))
    };
}
