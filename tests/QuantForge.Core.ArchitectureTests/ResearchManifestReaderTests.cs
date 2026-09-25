using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchManifestReaderTests
{
    [Theory]
    [InlineData("missing-authority")]
    [InlineData("duplicate-authority")]
    [InlineData("unknown-property")]
    [InlineData("unknown-authority")]
    [InlineData("live-authority")]
    [InlineData("future-version")]
    [InlineData("null-identity")]
    [InlineData("long-identity")]
    public async Task AmbiguousOrUnsupportedManifest_IsRejectedWithoutMetadata(string failure)
    {
        var json = Canonical();
        var node = JsonNode.Parse(json)!.AsObject();
        switch (failure)
        {
            case "missing-authority": node.Remove("authorityDomain"); break;
            case "unknown-property": node["enableOrders"] = true; break;
            case "unknown-authority": node["authorityDomain"] = 999; break;
            case "live-authority": node["authorityDomain"] = 4; break;
            case "future-version": node["manifestVersion"] = "2"; break;
            case "null-identity": node["datasetId"] = null; break;
            case "long-identity": node["datasetId"] = new string('x', 513); break;
        }
        json = failure == "duplicate-authority"
            ? json.Replace("\"authorityDomain\":3", "\"authorityDomain\":4,\"authorityDomain\":3", StringComparison.Ordinal)
            : node.ToJsonString();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var result = await ResearchManifestReader.InspectAsync(stream);
        Assert.Equal(ManifestInspectionStatus.Invalid, result.Status);
        Assert.Null(result.Manifest);
        Assert.Null(result.SourceFingerprint);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ValidPartialReads_PreserveExactByteFingerprintAndLeaveStreamOpen(bool withBom)
    {
        var payload = Encoding.UTF8.GetBytes(Canonical());
        var bytes = withBom ? new byte[] { 0xEF, 0xBB, 0xBF }.Concat(payload).ToArray() : payload;
        using var stream = new ChunkedStream(bytes);
        var result = await ResearchManifestReader.InspectAsync(stream);
        Assert.Equal(ManifestInspectionStatus.Inspected, result.Status);
        Assert.Equal(Manifest(), result.Manifest);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(bytes)), result.SourceFingerprint);
        Assert.True(stream.CanRead);
    }

    [Fact]
    public async Task OversizedInput_StopsAfterLimitPlusOneByte()
    {
        using var stream = new MemoryStream(new byte[ResearchManifestCodec.MaximumBytes * 2]);
        var result = await ResearchManifestReader.InspectAsync(stream);
        Assert.Equal("QF-MANIFEST-TOO-LARGE", result.DiagnosticCode);
        Assert.Equal(ResearchManifestCodec.MaximumBytes + 1L, stream.Position);
        Assert.Null(result.Manifest);
    }

    [Fact]
    public async Task CancelledRead_DoesNotConsumeOrAdmitBytes()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Canonical()));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var result = await ResearchManifestReader.InspectAsync(stream, cancellation.Token);
        Assert.Equal(ManifestInspectionStatus.Cancelled, result.Status);
        Assert.Equal(0, stream.Position);
        Assert.Null(result.Manifest);
    }

    [Fact]
    public async Task ProviderFailure_IsExplicitAndDoesNotPoisonNextAttempt()
    {
        using var unavailable = new UnavailableStream();
        var failure = await ResearchManifestReader.InspectAsync(unavailable);
        Assert.Equal(ManifestInspectionStatus.Unavailable, failure.Status);
        Assert.Equal("QF-MANIFEST-UNAVAILABLE", failure.DiagnosticCode);
        Assert.Null(failure.Manifest);
        using var valid = new MemoryStream(Encoding.UTF8.GetBytes(Canonical()));
        Assert.Equal(ManifestInspectionStatus.Inspected, (await ResearchManifestReader.InspectAsync(valid)).Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData("[]")]
    [InlineData("{broken")]
    [InlineData("null")]
    public async Task CorruptOrEmptyInput_IsExplicitlyInvalid(string json)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var result = await ResearchManifestReader.InspectAsync(stream);
        Assert.Equal(ManifestInspectionStatus.Invalid, result.Status);
        Assert.Null(result.Manifest);
    }

    [Fact]
    public async Task InvalidUtf8_IsNotSilentlyReplaced()
    {
        using var stream = new MemoryStream(new byte[] { 0xC3, 0x28 });
        Assert.Equal(ManifestInspectionStatus.Invalid, (await ResearchManifestReader.InspectAsync(stream)).Status);
    }

    [Fact]
    public async Task ClosedStream_ReturnsUnavailable()
    {
        var stream = new MemoryStream();
        stream.Dispose();
        Assert.Equal(ManifestInspectionStatus.Unavailable, (await ResearchManifestReader.InspectAsync(stream)).Status);
    }

    [Fact]
    public void CanonicalWriter_RejectsUndefinedAuthority()
    {
        Assert.Throws<InvalidOperationException>(() => ResearchManifestCodec.SerializeCanonical(
            Manifest() with { AuthorityDomain = (AuthorityDomain)999 }));
    }

    private static ResearchManifest Manifest() => new("1", "dataset", "data-sha", "strategy", "strategy-sha",
        "execution-sha", "parameters-sha", "partition", "job-sha", AuthorityDomain.ReadOnlyResearch);
    private static string Canonical() => ResearchManifestCodec.SerializeCanonical(Manifest());

    private sealed class ChunkedStream(byte[] bytes) : MemoryStream(bytes)
    {
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            base.ReadAsync(buffer[..Math.Min(buffer.Length, 3)], cancellationToken);
    }

    private sealed class UnavailableStream : MemoryStream
    {
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            throw new IOException("Private provider/path detail must not be displayed.");
    }
}
