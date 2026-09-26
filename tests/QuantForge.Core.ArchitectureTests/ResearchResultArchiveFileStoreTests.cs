using System.Text;
using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchResultArchiveFileStoreTests
{
    [Fact]
    public void Stored_result_verification_rejects_tampered_chart()
    {
        var root = Path.Combine(Path.GetTempPath(), "qf-archive-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var artifact = TestArtifact();
            var store = new ResearchResultArchiveFileStore(root);
            var path = store.Save(artifact);
            store.VerifyStored(artifact.ArtifactFingerprint);
            File.AppendAllText(Path.Combine(path, "chart.csv"), "tamper", Encoding.UTF8);
            Assert.Throws<InvalidOperationException>(() => store.VerifyStored(artifact.ArtifactFingerprint));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    private static ResearchPublicationArtifact TestArtifact()
    {
        // The storage invariant is exercised independently of the full execution fixture.
        var report = new ResearchReport("job", ResearchResultStatus.Complete, "dataset", "strategy", "timing", "params", "wf", "ok",
            new EvidenceChainTail("evidence", "tail"),
            new ResearchExecutionTrace("trace", Array.Empty<ExecutionFill>(), Array.Empty<EquityPoint>(), Array.Empty<LedgerEntry>()));
        var reliability = new DataReliabilityAssessment("dataset", true, 0, 0, "validated");
        var publication = new ResearchPublicationBundle(report,
            new ResearchProvenanceBundle(new ProvenanceRecord("a", "fixture://strategy", "2026-09-26T00:00:00Z", "s", "o", "strategy", "scrub"), "dataset", "job", "evidence"),
            reliability);
        return ResearchPublicationArtifactFactory.Create(publication,
            new ResearchResultPipelineOutput(report, report.ExecutionTrace, "chart", "ledger"));
    }
}
