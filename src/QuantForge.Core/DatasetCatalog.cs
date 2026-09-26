namespace QuantForge.Core;

public readonly record struct DatasetCatalogEntry(
    string DatasetId,
    string DatasetFingerprint,
    string Instrument,
    string Timeframe,
    DateTimeOffset Start,
    DateTimeOffset End,
    bool StructuralValidationPassed,
    ProvenanceRecord Provenance,
    string InspectionFingerprint);

public sealed class DatasetCatalog
{
    private readonly Dictionary<string, DatasetCatalogEntry> _entries = new(StringComparer.Ordinal);

    public int Count => _entries.Count;

    public void Register(DatasetCatalogEntry entry)
    {
        RequireRegisterable(entry);

        if (_entries.TryGetValue(entry.DatasetId, out var existing))
        {
            if (existing != entry)
                throw new InvalidOperationException("Dataset ID is already registered with different immutable identity.");

            return;
        }

        _entries.Add(entry.DatasetId, entry);
    }

    public bool TryGet(string datasetId, out DatasetCatalogEntry entry) =>
        _entries.TryGetValue(datasetId, out entry);

    public IReadOnlyList<DatasetCatalogEntry> List() =>
        _entries.Values.OrderBy(x => x.DatasetId, StringComparer.Ordinal).ToArray();

    public DataAdmission RequireAdmission(string datasetId)
    {
        if (!_entries.TryGetValue(datasetId, out var entry))
            throw new InvalidOperationException("Dataset is not registered in the catalog.");

        RequireRegisterable(entry);
        return new DataAdmission(
            entry.DatasetId,
            entry.DatasetFingerprint,
            entry.Instrument,
            entry.Timeframe,
            entry.Start,
            entry.End,
            entry.StructuralValidationPassed,
            true);
    }

    private static void RequireRegisterable(DatasetCatalogEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.DatasetId) ||
            string.IsNullOrWhiteSpace(entry.DatasetFingerprint) ||
            string.IsNullOrWhiteSpace(entry.Instrument) ||
            string.IsNullOrWhiteSpace(entry.Timeframe) ||
            string.IsNullOrWhiteSpace(entry.InspectionFingerprint))
            throw new InvalidOperationException("Dataset catalog identity is incomplete.");

        if (entry.End < entry.Start)
            throw new InvalidOperationException("Dataset catalog range is invalid.");

        if (!entry.StructuralValidationPassed)
            throw new InvalidOperationException("Dataset catalog admission requires successful structural validation.");

        ProvenanceRules.RequireComplete(entry.Provenance);

        if (!string.Equals(entry.DatasetFingerprint, entry.Provenance.SanitizedArtifactFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Dataset fingerprint must match the immutable sanitized artifact fingerprint.");

        if (!string.Equals(entry.DatasetFingerprint, entry.InspectionFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Dataset fingerprint must match the inspected dataset fingerprint.");
    }
}
