using System.Text;
using System.Text.Json.Nodes;
using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class SessionCoverageEvidenceTests
{
    [Fact]
    public void CanonicalEvidence_RoundTripsWithExplicitUtcSessions()
    {
        var evidence = Evidence();
        var json = SessionCoverageEvidenceCodec.SerializeCanonical(evidence);
        var decoded = SessionCoverageEvidenceCodec.DeserializeAndValidate(json);
        Assert.Equal(evidence.DatasetId, decoded.DatasetId);
        Assert.Equal(evidence.DatasetFingerprint, decoded.DatasetFingerprint);
        Assert.True(decoded.Policy.Authoritative);
        Assert.Single(decoded.Policy.Sessions);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("future")]
    [InlineData("non-authoritative")]
    public void AmbiguousOrNonAuthoritativeEvidence_IsRejected(string mode)
    {
        var json = SessionCoverageEvidenceCodec.SerializeCanonical(Evidence());
        var node = JsonNode.Parse(json)!.AsObject();
        if (mode == "unknown") node["coveragePassed"] = true;
        else if (mode == "future") node["schemaVersion"] = "2";
        else node["policy"]!.AsObject()["authoritative"] = false;
        Assert.ThrowsAny<Exception>(() => SessionCoverageEvidenceCodec.DeserializeAndValidate(node.ToJsonString()));
    }

    [Fact]
    public void Preparation_RecomputesCoverageFromExactAdmittedMinuteBars()
    {
        var hash = new string('A', 64);
        var evidence = new SessionCoverageEvidence(
            "1", "dataset-1", hash,
            new SessionCoveragePolicy("policy", "1", new string('B', 64),
                new[] { new SessionInterval(new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 9, 1, 0, 2, 0, TimeSpan.Zero)) }, true));
        var rows = new[]
        {
            new MarketTextRow(new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero), 1, 2, 0.5m, 1.5m, 10),
            new MarketTextRow(new DateTimeOffset(2026, 9, 1, 0, 1, 0, TimeSpan.Zero), 1.5m, 2, 1, 1.8m, 12)
        };
        var file = new MarketBatchFile(1, "MES 09-26.Minute.Last.txt", null, MarketFileState.Inspected,
            "QF-BATCH-INSPECTED", new Nt8MinuteDescriptor("MES 09-26", MarketPriceSeries.Last),
            "Unverified filename label", new MarketTextResult("QF-BATCH-INSPECTED", MarketTextKind.Minute, rows, hash, 10, 1));
        var provenance = new ProvenanceRecord("artifact", "file://external", "2026-09-26T00:00:00Z", hash, hash, hash, "scrub");
        var entry = new DatasetCatalogEntry("dataset-1", hash, "MES 09-26", "Minute", rows[0].Stamp, rows[^1].Stamp, true, provenance, hash);

        var report = SessionCoveragePreparation.Analyze(entry, file, evidence);
        Assert.True(report.ResearchAdmissible);
        Assert.Equal(2, report.ObservedInSessionMinuteCount);
        Assert.Equal(0, report.MissingMinuteCount);
    }

    [Fact]
    public void Preparation_RejectsDatasetOrByteMismatch()
    {
        var evidence = Evidence();
        var rows = new[] { new MarketTextRow(new DateTimeOffset(2026, 9, 1, 13, 30, 0, TimeSpan.Zero), 1, 2, 0.5m, 1.5m, 10) };
        var hash = new string('A', 64);
        var file = new MarketBatchFile(1, "MES 09-26.Minute.Last.txt", null, MarketFileState.Inspected,
            "QF-BATCH-INSPECTED", new Nt8MinuteDescriptor("MES 09-26", MarketPriceSeries.Last),
            "Unverified filename label", new MarketTextResult("QF-BATCH-INSPECTED", MarketTextKind.Minute, rows, hash, 10, 1));
        var provenance = new ProvenanceRecord("artifact", "file://external", "2026-09-26T00:00:00Z", hash, hash, hash, "scrub");
        var entry = new DatasetCatalogEntry("different-dataset", hash, "MES 09-26", "Minute", rows[0].Stamp, rows[0].Stamp, true, provenance, hash);
        Assert.Throws<InvalidOperationException>(() => SessionCoveragePreparation.Analyze(entry, file, evidence));
    }

    [Fact]
    public async Task Reader_IsBoundedAndDoesNotAssertCoverageResult()
    {
        var json = SessionCoverageEvidenceCodec.SerializeCanonical(Evidence());
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var result = await SessionCoverageEvidenceReader.InspectAsync(stream);
        Assert.Equal(SessionCoverageEvidenceStatus.Inspected, result.Status);
        Assert.NotNull(result.Evidence);
    }

    private static SessionCoverageEvidence Evidence() => new(
        "1", "dataset-1", new string('A', 64),
        new SessionCoveragePolicy("CME-MES-RTH", "2026-09-26", new string('B', 64),
            new[] { new SessionInterval(new DateTimeOffset(2026, 9, 1, 13, 30, 0, TimeSpan.Zero), new DateTimeOffset(2026, 9, 1, 20, 0, 0, TimeSpan.Zero)) }, true));
}
