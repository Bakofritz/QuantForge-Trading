namespace QuantForge.Core;

public sealed record MarketAdmissionCommitResult(
    DatasetCatalogEntry Entry,
    DataAdmission Admission,
    int CatalogCount);

/// <summary>
/// Application-facing admission workflow. It consumes an already prepared admission request,
/// revalidates the exact inspected bytes through the existing admission pipeline, persists the
/// catalog atomically, and returns the resulting read-only admission identity. It does not grant
/// strategy, research-execution, broker, order-submission, or live authority.
/// </summary>
public sealed class MarketAdmissionApplicationWorkflow
{
    private readonly DatasetCatalogFileStore _store;

    public MarketAdmissionApplicationWorkflow(DatasetCatalogFileStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public MarketAdmissionCommitResult Admit(MarketBatchAdmissionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entry = MarketBatchAdmissionPipeline.CreateCatalogEntry(request);
        var catalog = _store.Load();
        catalog.Register(entry);
        var admission = catalog.RequireAdmission(entry.DatasetId);
        _store.Save(catalog);
        return new(entry, admission, catalog.Count);
    }

    public IReadOnlyList<DatasetCatalogEntry> LoadEntries() => _store.Load().List();
}
