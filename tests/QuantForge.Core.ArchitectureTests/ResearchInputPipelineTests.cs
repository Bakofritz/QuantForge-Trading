using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchInputPipelineTests
{
    [Fact]
    public void Incomplete_batch_cannot_mutate_catalog()
    {
        var catalog = new DatasetCatalog();
        var file = new MarketBatchFile(1, "x.txt", null, MarketFileState.Rejected, "bad", null, "unresolved", null);
        var batch = new MarketBatchResult("QF-BATCH-CANCELLED", new[] { file });
        var request = new MarketBatchAdmissionRequest("id", "MES", "1m", default, file);

        Assert.Throws<InvalidOperationException>(() => ResearchInputPipeline.AdmitCompletedBatch(catalog, batch, new[] { request }));
        Assert.Equal(0, catalog.Count);
    }
}
