using System.Text;
using System.Text.Json.Nodes;

namespace QuantForge.Core.ArchitectureTests;

public sealed class MarketAdmissionEvidenceTests
{
    [Fact]
    public void ExactExternalEvidence_PreparesButDoesNotAutoAdmitRequest()
    {
        var file = InspectedFile();
        var evidence = Evidence(file.Data!.Hash);
        var request = MarketAdmissionPreparation.Prepare(file, evidence);
        Assert.Equal("dataset-1", request.DatasetId);
        Assert.Equal(file, request.File);
        Assert.Equal(0, new DatasetCatalog().Count);
    }

    [Fact]
    public void FingerprintMismatch_CannotPrepareAdmissionRequest()
    {
        var file = InspectedFile();
        Assert.Throws<InvalidOperationException>(() =>
            MarketAdmissionPreparation.Prepare(file, Evidence(new string('B', 64))));
    }

    [Fact]
    public void FilenameIdentityConflict_CannotBeOverriddenByEvidence()
    {
        var file = InspectedFile();
        var evidence = Evidence(file.Data!.Hash) with { Instrument = "MNQ 09-26" };
        Assert.Throws<InvalidOperationException>(() => MarketAdmissionPreparation.Prepare(file, evidence));
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("duplicate")]
    [InlineData("missing")]
    [InlineData("future")]
    public void AmbiguousEvidence_IsRejected(string mode)
    {
        var evidence = Evidence(new string('A', 64));
        var json = MarketAdmissionEvidenceCodec.SerializeCanonical(evidence);
        var node = JsonNode.Parse(json)!.AsObject();
        json = mode switch
        {
            "unknown" => AddUnknown(node),
            "duplicate" => json.Replace("\"datasetId\":\"dataset-1\"", "\"datasetId\":\"dataset-1\",\"datasetId\":\"dataset-2\"", StringComparison.Ordinal),
            "missing" => Remove(node, "instrument"),
            "future" => Set(node, "schemaVersion", "2"),
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };
        Assert.ThrowsAny<Exception>(() => MarketAdmissionEvidenceCodec.DeserializeAndValidate(json));
    }

    [Fact]
    public async Task Reader_IsBoundedAndDoesNotAdmit()
    {
        var file = InspectedFile();
        var json = MarketAdmissionEvidenceCodec.SerializeCanonical(Evidence(file.Data!.Hash));
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var inspected = await MarketAdmissionEvidenceReader.InspectAsync(stream);
        Assert.Equal(MarketAdmissionEvidenceStatus.Inspected, inspected.Status);
        Assert.NotNull(inspected.Evidence);
        Assert.NotNull(inspected.SourceFingerprint);
        Assert.Equal(0, new DatasetCatalog().Count);
    }

    private static MarketAdmissionEvidence Evidence(string fingerprint) => new(
        "1", "dataset-1", "MES 09-26", "Minute",
        new("artifact-1", "file://external", "2026-09-26T00:00:00Z", fingerprint,
            fingerprint, fingerprint, "scrub-1"));

    private static MarketBatchFile InspectedFile()
    {
        var rows = new[]
        {
            new MarketTextRow(new DateTimeOffset(2026, 9, 1, 0, 1, 0, TimeSpan.Zero), 1, 2, 0.5m, 1.5m, 10)
        };
        var hash = new string('A', 64);
        var data = new MarketTextResult("QF-BATCH-INSPECTED", MarketTextKind.Minute, rows, hash, 10, 1);
        return new MarketBatchFile(1, "MES 09-26.Minute.Last.txt", null, MarketFileState.Inspected,
            "QF-BATCH-INSPECTED", new Nt8MinuteDescriptor("MES 09-26", MarketPriceSeries.Last),
            "Unverified filename label", data);
    }

    private static string AddUnknown(JsonObject node) { node["admitted"] = true; return node.ToJsonString(); }
    private static string Remove(JsonObject node, string name) { node.Remove(name); return node.ToJsonString(); }
    private static string Set(JsonObject node, string name, string value) { node[name] = value; return node.ToJsonString(); }
}
