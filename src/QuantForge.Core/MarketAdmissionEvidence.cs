using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuantForge.Core;

public sealed record MarketAdmissionEvidence(
    string SchemaVersion,
    string DatasetId,
    string Instrument,
    string Timeframe,
    ProvenanceRecord Provenance);

public enum MarketAdmissionEvidenceStatus { Inspected, Invalid, Unavailable, Cancelled }

public sealed record MarketAdmissionEvidenceInspection(
    MarketAdmissionEvidenceStatus Status,
    string DiagnosticCode,
    MarketAdmissionEvidence? Evidence = null,
    string? SourceFingerprint = null);

/// <summary>
/// Strict, bounded reader for independently supplied market-data admission evidence.
/// Reading evidence never admits a dataset; exact inspected bytes must still agree.
/// </summary>
public static class MarketAdmissionEvidenceCodec
{
    public const int MaximumBytes = 65536;
    private static readonly HashSet<string> RequiredProperties = new(StringComparer.Ordinal)
    {
        "schemaVersion", "datasetId", "instrument", "timeframe", "provenance"
    };
    private static readonly HashSet<string> RequiredProvenanceProperties = new(StringComparer.Ordinal)
    {
        "artifactId", "sourceUri", "retrievedAtUtc", "sha256", "originalArtifactFingerprint",
        "sanitizedArtifactFingerprint", "scrubReportId"
    };

    public static string SerializeCanonical(MarketAdmissionEvidence evidence)
    {
        RequireComplete(evidence);
        return JsonSerializer.Serialize(evidence, MarketAdmissionEvidenceJsonContext.Default.MarketAdmissionEvidence);
    }

    public static MarketAdmissionEvidence DeserializeAndValidate(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException("Market admission evidence JSON is required.");
        if (Encoding.UTF8.GetByteCount(json) > MaximumBytes)
            throw new InvalidOperationException("Market admission evidence exceeds the 64 KiB limit.");

        using var document = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 8 });
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("Market admission evidence must be a JSON object.");
        RequireExactProperties(document.RootElement, RequiredProperties, "Market admission evidence");

        if (!document.RootElement.TryGetProperty("provenance", out var provenance) || provenance.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("Market admission provenance must be an explicit object.");
        RequireExactProperties(provenance, RequiredProvenanceProperties, "Market admission provenance");

        var evidence = JsonSerializer.Deserialize(json, MarketAdmissionEvidenceJsonContext.Default.MarketAdmissionEvidence)
            ?? throw new InvalidOperationException("Market admission evidence could not be decoded.");
        RequireComplete(evidence);
        return evidence;
    }

    private static void RequireExactProperties(JsonElement element, HashSet<string> required, string label)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (!required.Contains(property.Name) || !seen.Add(property.Name))
                throw new InvalidOperationException($"{label} contains unknown or duplicate properties.");
        }
        if (!seen.SetEquals(required))
            throw new InvalidOperationException($"Every {label.ToLowerInvariant()} property must be explicit.");
    }

    private static void RequireComplete(MarketAdmissionEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        if (evidence.SchemaVersion != "1")
            throw new InvalidOperationException("Unsupported market admission evidence version.");
        if (new[] { evidence.DatasetId, evidence.Instrument, evidence.Timeframe }
            .Any(x => string.IsNullOrWhiteSpace(x) || x.Length > 512))
            throw new InvalidOperationException("Market admission identity is incomplete or too long.");
        ProvenanceRules.RequireComplete(evidence.Provenance);
    }
}

public static class MarketAdmissionEvidenceReader
{
    public static async Task<MarketAdmissionEvidenceInspection> InspectAsync(
        Stream source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        try
        {
            var bytes = new byte[MarketAdmissionEvidenceCodec.MaximumBytes + 1];
            var count = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var read = await source.ReadAsync(bytes.AsMemory(count, bytes.Length - count), cancellationToken).ConfigureAwait(false);
                if (read == 0) break;
                count += read;
                if (count > MarketAdmissionEvidenceCodec.MaximumBytes)
                    return new(MarketAdmissionEvidenceStatus.Invalid, "QF-ADMISSION-EVIDENCE-TOO-LARGE");
            }
            cancellationToken.ThrowIfCancellationRequested();
            var offset = count >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;
            var json = new UTF8Encoding(false, true).GetString(bytes, offset, count - offset);
            var evidence = MarketAdmissionEvidenceCodec.DeserializeAndValidate(json);
            var fingerprint = Convert.ToHexString(SHA256.HashData(bytes.AsSpan(0, count)));
            return new(MarketAdmissionEvidenceStatus.Inspected, "QF-ADMISSION-EVIDENCE-INSPECTED", evidence, fingerprint);
        }
        catch (OperationCanceledException)
        {
            return new(MarketAdmissionEvidenceStatus.Cancelled, "QF-ADMISSION-EVIDENCE-CANCELLED");
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or ObjectDisposedException or NotSupportedException)
        {
            return new(MarketAdmissionEvidenceStatus.Unavailable, "QF-ADMISSION-EVIDENCE-UNAVAILABLE");
        }
        catch (Exception error) when (error is JsonException or InvalidOperationException or DecoderFallbackException)
        {
            return new(MarketAdmissionEvidenceStatus.Invalid, "QF-ADMISSION-EVIDENCE-INVALID");
        }
    }
}

/// <summary>
/// Pairs independently supplied evidence with one completed inspected source by exact fingerprint.
/// It prepares an admission request but does not mutate a catalog.
/// </summary>
public static class MarketAdmissionPreparation
{
    public static MarketBatchAdmissionRequest Prepare(MarketBatchFile file, MarketAdmissionEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(evidence);
        // Canonical serialization performs the complete evidence validation without granting authority.
        _ = MarketAdmissionEvidenceCodec.SerializeCanonical(evidence);
        var readiness = MarketAdmissionReadinessRules.Assess(
            file, evidence.DatasetId, evidence.Instrument, evidence.Timeframe, evidence.Provenance);
        if (!readiness.CanCreateAdmissionRequest)
            throw new InvalidOperationException("Inspected source and external evidence are not ready to form an admission request.");
        return new(evidence.DatasetId, evidence.Instrument, evidence.Timeframe, evidence.Provenance, file);
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
[JsonSerializable(typeof(MarketAdmissionEvidence))]
internal partial class MarketAdmissionEvidenceJsonContext : JsonSerializerContext { }
