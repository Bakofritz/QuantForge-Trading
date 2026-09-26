namespace QuantForge.Core;

/// <summary>
/// Transactional bridge from a completed structural market-batch inspection to the
/// persistent dataset catalog. It never upgrades filename labels into provenance.
/// </summary>
public static class ResearchInputPipeline
{
    public static DatasetCatalog AdmitCompletedBatch(
        DatasetCatalog catalog,
        MarketBatchResult batch,
        IEnumerable<MarketBatchAdmissionRequest> admissions)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(batch);
        ArgumentNullException.ThrowIfNull(admissions);

        if (!batch.Completed)
            throw new InvalidOperationException("Only a completed market batch can be admitted to the research catalog.");

        var files = batch.Files.ToDictionary(x => x.Index);
        var requests = admissions.ToArray();
        var prepared = new List<MarketBatchAdmissionRequest>(requests.Length);
        foreach (var request in requests)
        {
            if (!files.TryGetValue(request.File.Index, out var actual) || actual != request.File)
                throw new InvalidOperationException("Admission request does not exactly match the completed batch inspection result.");
            prepared.Add(request);
        }

        var staged = new List<DatasetCatalogEntry>(prepared.Count);
        foreach (var request in prepared)
            staged.Add(MarketBatchAdmissionPipeline.CreateCatalogEntry(request));

        // Validate the complete batch before mutating the catalog.
        foreach (var entry in staged)
            catalog.Register(entry);

        return catalog;
    }
}
