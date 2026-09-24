using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public class ManifestTests
{
    [Fact]
    public void Research_manifest_round_trips()
    {
        var manifest = new ResearchManifest(
            "1",
            "MES-NT8-MINUTE",
            "dataset-sha256",
            "strategy-a",
            "strategy-sha256",
            "execution-sha256",
            "parameters-sha256",
            "walk-forward-01",
            "job-sha256",
            AuthorityDomain.ReadOnlyResearch);

        var json = ResearchManifestCodec.SerializeCanonical(manifest);
        var decoded = ResearchManifestCodec.DeserializeAndValidate(json);

        Assert.Equal(manifest, decoded);
    }

    [Fact]
    public void Live_authority_is_not_serializable_as_research()
    {
        var manifest = new ResearchManifest(
            "1", "d", "d", "s", "s", "e", "p", "t", "j",
            AuthorityDomain.LiveAccount);

        Assert.Throws<InvalidOperationException>(
            () => ResearchManifestCodec.SerializeCanonical(manifest));
    }
}
