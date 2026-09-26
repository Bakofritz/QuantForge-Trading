using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace QuantForge.Core;

public sealed record StrategyAdmissionArtifact(
    string ArtifactFingerprint,
    StrategyAdmissionEnvelope Envelope);

public static class StrategyAdmissionArtifactRules
{
    public static StrategyAdmissionArtifact Create(StrategyAdmissionEnvelope envelope)
    {
        StrategyAdmissionPipeline.RequireAdmitted(envelope);
        return new(Fingerprint(envelope), envelope);
    }

    public static void Validate(StrategyAdmissionArtifact artifact)
    {
        ArgumentNullException.ThrowIfNull(artifact);
        StrategyAdmissionPipeline.RequireAdmitted(artifact.Envelope);
        if (!string.Equals(Fingerprint(artifact.Envelope), artifact.ArtifactFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Strategy admission artifact fingerprint does not match its admitted envelope.");
    }

    private static string Fingerprint(StrategyAdmissionEnvelope envelope)
    {
        var features = envelope.Selection.EnabledFeatures
            .OrderBy(x => (int)x)
            .Select(x => x.ToString());
        var text = string.Join("|", new[]
        {
            envelope.State.ToString(),
            envelope.Manifest.StrategyId,
            envelope.Manifest.SourceFingerprint,
            envelope.Manifest.CanSubmitOrders.ToString(),
            envelope.Manifest.CanChangeApplicationSettings.ToString(),
            envelope.Manifest.UsesNetwork.ToString(),
            envelope.Manifest.UsesFilesystem.ToString(),
            envelope.Manifest.UsesProcessExecution.ToString(),
            envelope.Manifest.UsesNativeLibrary.ToString(),
            envelope.Manifest.RequiresLiveAccount.ToString(),
            envelope.QuarantineFingerprint,
            envelope.SanitizedFingerprint,
            string.Join(",", features)
        });
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
    }
}

public sealed class StrategyAdmissionArtifactFileStore
{
    private readonly string _rootDirectory;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public StrategyAdmissionArtifactFileStore(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("A strategy admission artifact directory is required.", nameof(rootDirectory));
        _rootDirectory = Path.GetFullPath(rootDirectory);
    }

    public string Save(StrategyAdmissionArtifact artifact)
    {
        StrategyAdmissionArtifactRules.Validate(artifact);
        var directory = Path.Combine(_rootDirectory, SafeSegment(artifact.ArtifactFingerprint));
        var path = Path.Combine(directory, "strategy-admission.json");
        var json = JsonSerializer.Serialize(artifact, JsonOptions);
        Directory.CreateDirectory(_rootDirectory);
        if (Directory.Exists(directory))
        {
            if (!File.Exists(path)) throw new InvalidOperationException("Strategy admission artifact directory is incomplete.");
            if (!string.Equals(File.ReadAllText(path, Encoding.UTF8), json, StringComparison.Ordinal))
                throw new InvalidOperationException("Strategy admission artifact fingerprint collision contains different content.");
            return path;
        }
        var temp = directory + ".tmp-" + Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(temp);
        try
        {
            File.WriteAllText(Path.Combine(temp, "strategy-admission.json"), json, new UTF8Encoding(false));
            Directory.Move(temp, directory);
            return path;
        }
        catch
        {
            if (Directory.Exists(temp)) Directory.Delete(temp, true);
            throw;
        }
    }

    public StrategyAdmissionArtifact Load(string artifactFingerprint)
    {
        var path = Path.Combine(_rootDirectory, SafeSegment(artifactFingerprint), "strategy-admission.json");
        if (!File.Exists(path)) throw new FileNotFoundException("Strategy admission artifact does not exist.", path);
        var artifact = JsonSerializer.Deserialize<StrategyAdmissionArtifact>(File.ReadAllText(path, Encoding.UTF8), JsonOptions)
            ?? throw new InvalidOperationException("Strategy admission artifact JSON is invalid.");
        if (!string.Equals(artifact.ArtifactFingerprint, artifactFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Strategy admission artifact identity does not match its storage key.");
        StrategyAdmissionArtifactRules.Validate(artifact);
        return artifact;
    }

    private static string SafeSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Any(char.IsWhiteSpace) || value.Any(ch => !(char.IsLetterOrDigit(ch) || ch is '-' or '_')))
            throw new ArgumentException("Strategy admission fingerprint is not a safe storage identifier.", nameof(value));
        return value;
    }
}
