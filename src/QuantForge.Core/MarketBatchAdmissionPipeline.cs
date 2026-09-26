namespace QuantForge.Core;

public sealed record MarketBatchAdmissionRequest(
    string DatasetId,
    string Instrument,
    string Timeframe,
    ProvenanceRecord Provenance,
    MarketBatchFile File);

/// <summary>
/// Converts structurally inspected market text into a catalog entry only when
/// an external, immutable provenance record binds the exact inspected bytes.
/// Filename labels remain descriptive evidence and never establish identity.
/// </summary>
public static class MarketBatchAdmissionPipeline
{
    public static DatasetCatalogEntry CreateCatalogEntry(MarketBatchAdmissionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.DatasetId) ||
            string.IsNullOrWhiteSpace(request.Instrument) ||
            string.IsNullOrWhiteSpace(request.Timeframe))
            throw new InvalidOperationException("Dataset identity must be supplied independently of the filename label.");

        var file = request.File;
        if (file.State != MarketFileState.Inspected || file.Data is null || !file.Data.Complete ||
            string.IsNullOrWhiteSpace(file.Data.Hash) || file.Data.Rows is null || file.Data.Rows.Count == 0)
            throw new InvalidOperationException("Only a completely inspected market file can enter data admission.");

        ProvenanceRules.RequireComplete(request.Provenance);
        if (!string.Equals(file.Data.Hash, request.Provenance.SanitizedArtifactFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Inspected market bytes do not match the immutable sanitized artifact fingerprint.");

        if (file.Label is not null &&
            !string.Equals(file.Label.Instrument, request.Instrument, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Externally supplied instrument identity conflicts with the descriptive filename label.");

        var rows = file.Data.Rows;
        return new DatasetCatalogEntry(
            request.DatasetId,
            file.Data.Hash,
            request.Instrument,
            request.Timeframe,
            rows[0].Stamp,
            rows[^1].Stamp,
            true,
            request.Provenance,
            file.Data.Hash);
    }

    public static DatasetCatalog AdmitIntoCatalog(
        DatasetCatalog catalog,
        IEnumerable<MarketBatchAdmissionRequest> requests)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(requests);

        foreach (var request in requests)
            catalog.Register(CreateCatalogEntry(request));

        return catalog;
    }
}
