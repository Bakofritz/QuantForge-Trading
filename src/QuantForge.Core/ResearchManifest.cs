using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuantForge.Core;

public sealed record ResearchManifest(
    string ManifestVersion,
    string DatasetId,
    string DatasetFingerprint,
    string StrategyId,
    string StrategyFingerprint,
    string ExecutionPolicyFingerprint,
    string ParameterSetFingerprint,
    string TemporalPartitionId,
    string JobFingerprint,
    AuthorityDomain AuthorityDomain);

public static class ResearchManifestCodec
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    public static string SerializeCanonical(ResearchManifest manifest)
    {
        RequireComplete(manifest);
        return JsonSerializer.Serialize(manifest, Options);
    }

    public static ResearchManifest DeserializeAndValidate(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException("Research manifest JSON is required.");

        var manifest = JsonSerializer.Deserialize<ResearchManifest>(json, Options)
            ?? throw new InvalidOperationException("Research manifest could not be decoded.");

        RequireComplete(manifest);
        return manifest;
    }

    private static void RequireComplete(ResearchManifest manifest)
    {
        var values = new[]
        {
            manifest.ManifestVersion,
            manifest.DatasetId,
            manifest.DatasetFingerprint,
            manifest.StrategyId,
            manifest.StrategyFingerprint,
            manifest.ExecutionPolicyFingerprint,
            manifest.ParameterSetFingerprint,
            manifest.TemporalPartitionId,
            manifest.JobFingerprint
        };

        if (values.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException(
                "Research manifest contains an incomplete reproducibility identity.");

        if (manifest.AuthorityDomain == AuthorityDomain.LiveAccount)
            throw new InvalidOperationException(
                "Live-account authority cannot be represented by a research manifest.");
    }
}
