using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class MarketBatchAdmissionPipelineTests
{
    private static ProvenanceRecord Provenance(string hash) => new(
        "artifact-01", "fixture://market", "2026-09-26T00:00:00Z", "source-sha", "original-sha", hash, "scrub-01");

    private static MarketBatchFile ValidFile(string hash = "dataset-sha")
    {
        var rows = new[]
        {
            new MarketTextRow(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), 1, 2, 0, 1, 10),
            new MarketTextRow(new DateTimeOffset(2026, 1, 1, 0, 1, 0, TimeSpan.Zero), 1, 3, 0, 2, 12)
        };
        var data = new MarketTextResult("QF-BATCH-INSPECTED", MarketTextKind.Minute, rows, hash, 10, 1);
        return new MarketBatchFile(1, "MES 09-26.Minute.Last.txt", null, MarketFileState.Inspected,
            "QF-BATCH-INSPECTED", new Nt8MinuteDescriptor("MES 09-26", MarketPriceSeries.Last),
            "Unverified filename label", data);
    }

    [Fact]
    public void Exact_inspected_bytes_and_external_provenance_create_catalog_entry()
    {
        var entry = MarketBatchAdmissionPipeline.CreateCatalogEntry(new(
            "MES-DATASET", "MES 09-26", "1m", Provenance("dataset-sha"), ValidFile()));

        Assert.Equal("dataset-sha", entry.DatasetFingerprint);
        Assert.Equal(entry.Start, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        Assert.Equal(entry.End, new DateTimeOffset(2026, 1, 1, 0, 1, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Unresolved_or_rejected_file_cannot_enter_admission()
    {
        var file = ValidFile() with { State = MarketFileState.UnresolvedIdentity };
        Assert.Throws<InvalidOperationException>(() => MarketBatchAdmissionPipeline.CreateCatalogEntry(
            new("MES-DATASET", "MES 09-26", "1m", Provenance("dataset-sha"), file)));
    }

    [Fact]
    public void Sanitized_fingerprint_mismatch_blocks_admission()
    {
        Assert.Throws<InvalidOperationException>(() => MarketBatchAdmissionPipeline.CreateCatalogEntry(
            new("MES-DATASET", "MES 09-26", "1m", Provenance("different"), ValidFile())));
    }

    [Fact]
    public void Conflicting_external_identity_is_not_overridden_by_filename()
    {
        Assert.Throws<InvalidOperationException>(() => MarketBatchAdmissionPipeline.CreateCatalogEntry(
            new("MES-DATASET", "MNQ 09-26", "1m", Provenance("dataset-sha"), ValidFile())));
    }
}
