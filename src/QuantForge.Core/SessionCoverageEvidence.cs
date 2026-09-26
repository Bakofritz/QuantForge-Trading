using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuantForge.Core;

public sealed record SessionCoverageEvidence(
    string SchemaVersion,
    string DatasetId,
    string DatasetFingerprint,
    SessionCoveragePolicy Policy);

public enum SessionCoverageEvidenceStatus { Inspected, Invalid, Unavailable, Cancelled }

public sealed record SessionCoverageEvidenceInspection(
    SessionCoverageEvidenceStatus Status,
    string DiagnosticCode,
    SessionCoverageEvidence? Evidence = null);

/// <summary>
/// Strict reader for independently supplied session-policy evidence. The document identifies the
/// dataset and the explicit UTC policy to test; QuantForge still recomputes coverage from inspected
/// bars and never trusts an externally asserted coverage outcome.
/// </summary>
public static class SessionCoverageEvidenceCodec
{
    public const int MaximumBytes = 262144;
    public const int MaximumSessions = 10000;

    private static readonly HashSet<string> RootProperties = new(StringComparer.Ordinal)
    {
        "schemaVersion", "datasetId", "datasetFingerprint", "policy"
    };
    private static readonly HashSet<string> PolicyProperties = new(StringComparer.Ordinal)
    {
        "policyId", "version", "provenanceFingerprint", "sessions", "authoritative"
    };
    private static readonly HashSet<string> SessionProperties = new(StringComparer.Ordinal)
    {
        "start", "end"
    };

    public static string SerializeCanonical(SessionCoverageEvidence evidence)
    {
        RequireComplete(evidence);
        return JsonSerializer.Serialize(evidence, SessionCoverageEvidenceJsonContext.Default.SessionCoverageEvidence);
    }

    public static SessionCoverageEvidence DeserializeAndValidate(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException("Session coverage evidence JSON is required.");
        if (Encoding.UTF8.GetByteCount(json) > MaximumBytes)
            throw new InvalidOperationException("Session coverage evidence exceeds the 256 KiB limit.");

        using var document = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 16 });
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("Session coverage evidence must be a JSON object.");
        RequireExact(document.RootElement, RootProperties, "Session coverage evidence");

        var policyElement = document.RootElement.GetProperty("policy");
        if (policyElement.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("Session coverage policy must be an object.");
        RequireExact(policyElement, PolicyProperties, "Session coverage policy");

        var sessions = policyElement.GetProperty("sessions");
        if (sessions.ValueKind != JsonValueKind.Array || sessions.GetArrayLength() == 0 || sessions.GetArrayLength() > MaximumSessions)
            throw new InvalidOperationException("Session coverage evidence requires a bounded non-empty session array.");
        foreach (var session in sessions.EnumerateArray())
        {
            if (session.ValueKind != JsonValueKind.Object)
                throw new InvalidOperationException("Session interval must be an object.");
            RequireExact(session, SessionProperties, "Session interval");
        }

        var evidence = JsonSerializer.Deserialize(json, SessionCoverageEvidenceJsonContext.Default.SessionCoverageEvidence)
            ?? throw new InvalidOperationException("Session coverage evidence could not be decoded.");
        RequireComplete(evidence);
        return evidence;
    }

    private static void RequireExact(JsonElement element, HashSet<string> expected, string label)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (!expected.Contains(property.Name) || !seen.Add(property.Name))
                throw new InvalidOperationException($"{label} contains unknown or duplicate properties.");
        }
        if (!seen.SetEquals(expected))
            throw new InvalidOperationException($"Every {label.ToLowerInvariant()} property must be explicit.");
    }

    private static void RequireComplete(SessionCoverageEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        if (evidence.SchemaVersion != "1")
            throw new InvalidOperationException("Unsupported session coverage evidence version.");
        if (string.IsNullOrWhiteSpace(evidence.DatasetId) || evidence.DatasetId.Length > 512 ||
            string.IsNullOrWhiteSpace(evidence.DatasetFingerprint) || evidence.DatasetFingerprint.Length > 512)
            throw new InvalidOperationException("Session coverage dataset identity is incomplete or too long.");
        if (evidence.Policy is null)
            throw new InvalidOperationException("Session coverage policy is required.");
        if (!evidence.Policy.Authoritative)
            throw new InvalidOperationException("External session coverage evidence must explicitly declare authoritative policy provenance.");
        if (evidence.Policy.Sessions is null || evidence.Policy.Sessions.Count == 0 || evidence.Policy.Sessions.Count > MaximumSessions)
            throw new InvalidOperationException("Session coverage evidence requires a bounded non-empty session list.");

        // Analyze validates policy identity, UTC timestamps, ordering and overlap without inventing sessions.
        foreach (var session in evidence.Policy.Sessions)
            session.Validate();
        var ordered = evidence.Policy.Sessions.OrderBy(x => x.Start).ToArray();
        for (var i = 1; i < ordered.Length; i++)
            if (ordered[i].Start < ordered[i - 1].End)
                throw new InvalidOperationException("Session intervals cannot overlap.");
        if (string.IsNullOrWhiteSpace(evidence.Policy.PolicyId) || string.IsNullOrWhiteSpace(evidence.Policy.Version) ||
            string.IsNullOrWhiteSpace(evidence.Policy.ProvenanceFingerprint))
            throw new InvalidOperationException("Session policy identity and provenance are required.");
    }
}

public static class SessionCoverageEvidenceReader
{
    public static async Task<SessionCoverageEvidenceInspection> InspectAsync(Stream source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        try
        {
            var bytes = new byte[SessionCoverageEvidenceCodec.MaximumBytes + 1];
            var count = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var read = await source.ReadAsync(bytes.AsMemory(count, bytes.Length - count), cancellationToken).ConfigureAwait(false);
                if (read == 0) break;
                count += read;
                if (count > SessionCoverageEvidenceCodec.MaximumBytes)
                    return new(SessionCoverageEvidenceStatus.Invalid, "QF-SESSION-EVIDENCE-TOO-LARGE");
            }
            var offset = count >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;
            var json = new UTF8Encoding(false, true).GetString(bytes, offset, count - offset);
            return new(SessionCoverageEvidenceStatus.Inspected, "QF-SESSION-EVIDENCE-INSPECTED",
                SessionCoverageEvidenceCodec.DeserializeAndValidate(json));
        }
        catch (OperationCanceledException)
        {
            return new(SessionCoverageEvidenceStatus.Cancelled, "QF-SESSION-EVIDENCE-CANCELLED");
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or ObjectDisposedException or NotSupportedException)
        {
            return new(SessionCoverageEvidenceStatus.Unavailable, "QF-SESSION-EVIDENCE-UNAVAILABLE");
        }
        catch (Exception error) when (error is JsonException or InvalidOperationException or DecoderFallbackException)
        {
            return new(SessionCoverageEvidenceStatus.Invalid, "QF-SESSION-EVIDENCE-INVALID");
        }
    }
}

public static class SessionCoveragePreparation
{
    public static SessionCoverageReport Analyze(
        DatasetCatalogEntry catalogEntry,
        MarketBatchFile inspectedFile,
        SessionCoverageEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(inspectedFile);
        ArgumentNullException.ThrowIfNull(evidence);
        _ = SessionCoverageEvidenceCodec.SerializeCanonical(evidence);

        var catalog = new DatasetCatalog();
        catalog.Register(catalogEntry);
        _ = catalog.RequireAdmission(catalogEntry.DatasetId);

        if (!string.Equals(catalogEntry.DatasetId, evidence.DatasetId, StringComparison.Ordinal) ||
            !string.Equals(catalogEntry.DatasetFingerprint, evidence.DatasetFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Session evidence dataset identity does not match the admitted catalog entry.");

        if (inspectedFile.State != MarketFileState.Inspected || inspectedFile.Data is not { Complete: true } data ||
            data.Kind != MarketTextKind.Minute || data.Rows is not { Count: > 0 } rows ||
            !string.Equals(data.Hash, catalogEntry.DatasetFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Authoritative session coverage requires the exact admitted inspected minute bars.");

        var events = rows.Select((row, index) => new MarketEvent(
            index, row.Stamp, row.Open, row.High, row.Low, row.Close, row.Volume)).ToArray();
        return SessionCoverageRules.Analyze(catalogEntry.DatasetFingerprint, events, evidence.Policy);
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
[JsonSerializable(typeof(SessionCoverageEvidence))]
internal partial class SessionCoverageEvidenceJsonContext : JsonSerializerContext { }
