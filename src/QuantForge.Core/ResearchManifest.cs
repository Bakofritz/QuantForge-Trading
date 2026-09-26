using System.Text;
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
    public const int MaximumBytes = 65536;
    private static readonly HashSet<string> RequiredProperties = new(StringComparer.Ordinal)
    {
        "manifestVersion", "datasetId", "datasetFingerprint", "strategyId",
        "strategyFingerprint", "executionPolicyFingerprint", "parameterSetFingerprint",
        "temporalPartitionId", "jobFingerprint", "authorityDomain"
    };

    public static string SerializeCanonical(ResearchManifest manifest)
    {
        RequireComplete(manifest);
        return JsonSerializer.Serialize(manifest, ResearchManifestJsonContext.Default.ResearchManifest);
    }

    public static ResearchManifest DeserializeAndValidate(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException("Research manifest JSON is required.");

        if (Encoding.UTF8.GetByteCount(json) > MaximumBytes)
            throw new InvalidOperationException("Research manifest exceeds the 64 KiB limit.");

        using var document = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 8 });
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("Research manifest must be a JSON object.");
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (!RequiredProperties.Contains(property.Name) || !seen.Add(property.Name))
                throw new InvalidOperationException("Research manifest contains unknown or duplicate properties.");
        }
        if (!seen.SetEquals(RequiredProperties))
            throw new InvalidOperationException("Every manifest property, including authority, must be explicit.");

        var manifest = JsonSerializer.Deserialize(json, ResearchManifestJsonContext.Default.ResearchManifest)
            ?? throw new InvalidOperationException("Research manifest could not be decoded.");

        RequireComplete(manifest);
        return manifest;
    }

    private static void RequireComplete(ResearchManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        if (manifest.ManifestVersion != "1")
            throw new InvalidOperationException("Unsupported research manifest version.");

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

        if (values.Any(x => string.IsNullOrWhiteSpace(x) || x.Length > 512))
            throw new InvalidOperationException(
                "Research manifest contains an incomplete reproducibility identity.");

        if (manifest.AuthorityDomain == AuthorityDomain.LiveAccount)
            throw new InvalidOperationException(
                "Live-account authority cannot be represented by a research manifest.");

        AuthorityBoundary.EvaluateResearch(manifest.AuthorityDomain);
    }
}


[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
[JsonSerializable(typeof(ResearchManifest))]
internal partial class ResearchManifestJsonContext : JsonSerializerContext { }
