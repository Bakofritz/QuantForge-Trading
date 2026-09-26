using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class MarketAdmissionApplicationWorkflowTests
{
    [Fact]
    public void VerifiedRequest_IsPersistedAndReloadedAsAdmission()
    {
        var path = TempCatalogPath();
        try
        {
            var workflow = new MarketAdmissionApplicationWorkflow(new DatasetCatalogFileStore(path));
            var result = workflow.Admit(Request("dataset-1", new string('A', 64)));

            Assert.Equal("dataset-1", result.Entry.DatasetId);
            Assert.Equal("dataset-1", result.Admission.DatasetId);
            Assert.True(result.Admission.StructuralValidationPassed);
            Assert.True(result.Admission.ProvenanceRecorded);
            Assert.Equal(1, result.CatalogCount);

            var entries = workflow.LoadEntries();
            Assert.Single(entries);
            Assert.Equal(result.Entry, entries[0]);
        }
        finally { TryDelete(path); }
    }

    [Fact]
    public void ExactDuplicate_IsIdempotentButConflictingDatasetIdFailsClosed()
    {
        var path = TempCatalogPath();
        try
        {
            var workflow = new MarketAdmissionApplicationWorkflow(new DatasetCatalogFileStore(path));
            var first = workflow.Admit(Request("dataset-1", new string('A', 64)));
            var duplicate = workflow.Admit(Request("dataset-1", new string('A', 64)));
            Assert.Equal(1, duplicate.CatalogCount);
            Assert.Equal(first.Entry, duplicate.Entry);

            Assert.Throws<InvalidOperationException>(() =>
                workflow.Admit(Request("dataset-1", new string('B', 64))));
            Assert.Single(workflow.LoadEntries());
        }
        finally { TryDelete(path); }
    }

    [Fact]
    public void InvalidPreparedRequest_CannotReachPersistentCatalog()
    {
        var path = TempCatalogPath();
        try
        {
            var workflow = new MarketAdmissionApplicationWorkflow(new DatasetCatalogFileStore(path));
            var request = Request("dataset-1", new string('A', 64));
            var invalid = request with { Provenance = request.Provenance with { SanitizedArtifactFingerprint = new string('B', 64) } };
            Assert.Throws<InvalidOperationException>(() => workflow.Admit(invalid));
            Assert.Empty(workflow.LoadEntries());
        }
        finally { TryDelete(path); }
    }

    private static MarketBatchAdmissionRequest Request(string datasetId, string hash)
    {
        var rows = new[]
        {
            new MarketTextRow(new DateTimeOffset(2026, 9, 1, 0, 1, 0, TimeSpan.Zero), 1, 2, 0.5m, 1.5m, 10)
        };
        var data = new MarketTextResult("QF-BATCH-INSPECTED", MarketTextKind.Minute, rows, hash, 10, 1);
        var file = new MarketBatchFile(1, "MES 09-26.Minute.Last.txt", null, MarketFileState.Inspected,
            "QF-BATCH-INSPECTED", new Nt8MinuteDescriptor("MES 09-26", MarketPriceSeries.Last),
            "Unverified filename label", data);
        var provenance = new ProvenanceRecord("artifact-1", "file://external", "2026-09-26T00:00:00Z", hash,
            hash, hash, "scrub-1");
        return new(datasetId, "MES 09-26", "Minute", provenance, file);
    }

    private static string TempCatalogPath() => Path.Combine(Path.GetTempPath(), "qf-catalog-" + Guid.NewGuid().ToString("N") + ".json");
    private static void TryDelete(string path) { if (File.Exists(path)) File.Delete(path); }
}
