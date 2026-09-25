using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace QuantForge.Core;

public enum ManifestInspectionStatus { Inspected, Invalid, Unavailable, Cancelled }

/// <summary>Inspection metadata only; never a data or strategy admission token.</summary>
public sealed record ManifestInspectionResult(
    ManifestInspectionStatus Status,
    string DiagnosticCode,
    ResearchManifest? Manifest = null,
    string? SourceFingerprint = null);

public static class ResearchManifestReader
{
    // Caller owns the stream. No filesystem paths, settings writes, or execution.
    public static async Task<ManifestInspectionResult> InspectAsync(
        Stream source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        try
        {
            var bytes = new byte[ResearchManifestCodec.MaximumBytes + 1];
            var count = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var read = await source.ReadAsync(bytes.AsMemory(count, bytes.Length - count), cancellationToken).ConfigureAwait(false);
                if (read == 0)
                    break;
                count += read;
                if (count > ResearchManifestCodec.MaximumBytes)
                    return new(ManifestInspectionStatus.Invalid, "QF-MANIFEST-TOO-LARGE");
            }
            cancellationToken.ThrowIfCancellationRequested();
            var offset = count >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;
            var json = new UTF8Encoding(false, true).GetString(bytes, offset, count - offset);
            var manifest = ResearchManifestCodec.DeserializeAndValidate(json);
            var fingerprint = Convert.ToHexString(SHA256.HashData(bytes.AsSpan(0, count)));
            return new(ManifestInspectionStatus.Inspected, "QF-MANIFEST-INSPECTED", manifest, fingerprint);
        }
        catch (OperationCanceledException)
        {
            return new(ManifestInspectionStatus.Cancelled, "QF-MANIFEST-CANCELLED");
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or ObjectDisposedException or NotSupportedException)
        {
            return new(ManifestInspectionStatus.Unavailable, "QF-MANIFEST-UNAVAILABLE");
        }
        catch (Exception error) when (error is JsonException or InvalidOperationException or DecoderFallbackException)
        {
            return new(ManifestInspectionStatus.Invalid, "QF-MANIFEST-INVALID");
        }
    }
}
