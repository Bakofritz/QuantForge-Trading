using System.Text;
using System.Text.Json;

namespace QuantForge.Core;

public sealed record ResearchOutcomePackage(
    ResearchLaunchPackage Launch,
    ResearchOutcomeArtifact Outcome);

public sealed class ResearchOutcomeArtifactFileStore
{
    private readonly string _rootDirectory;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public ResearchOutcomeArtifactFileStore(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("A research outcome directory is required.", nameof(rootDirectory));
        _rootDirectory = Path.GetFullPath(rootDirectory);
    }

    public string Save(ResearchOutcomePackage package)
    {
        ArgumentNullException.ThrowIfNull(package);
        ResearchGateBundleRules.Validate(package.Launch.Gates);
        ResearchLaunchIntentRules.Validate(package.Launch.Intent, package.Launch.Gates);
        ResearchOutcomeArtifactRules.Validate(package.Outcome, package.Launch.Intent);
        var directory = Path.Combine(_rootDirectory, SafeSegment(package.Outcome.ArtifactFingerprint));
        var path = Path.Combine(directory, "research-outcome.json");
        var json = JsonSerializer.Serialize(package, JsonOptions);
        Directory.CreateDirectory(_rootDirectory);
        if (Directory.Exists(directory))
        {
            if (!File.Exists(path)) throw new InvalidOperationException("Research outcome directory is incomplete.");
            if (!string.Equals(File.ReadAllText(path, Encoding.UTF8), json, StringComparison.Ordinal))
                throw new InvalidOperationException("Research outcome fingerprint collision contains different content.");
            return path;
        }
        var temp = directory + ".tmp-" + Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(temp);
        try
        {
            File.WriteAllText(Path.Combine(temp, "research-outcome.json"), json, new UTF8Encoding(false));
            Directory.Move(temp, directory);
            return path;
        }
        catch
        {
            if (Directory.Exists(temp)) Directory.Delete(temp, true);
            throw;
        }
    }

    public ResearchOutcomePackage Load(string artifactFingerprint)
    {
        var path = Path.Combine(_rootDirectory, SafeSegment(artifactFingerprint), "research-outcome.json");
        if (!File.Exists(path)) throw new FileNotFoundException("Research outcome package does not exist.", path);
        var package = JsonSerializer.Deserialize<ResearchOutcomePackage>(File.ReadAllText(path, Encoding.UTF8), JsonOptions)
            ?? throw new InvalidOperationException("Research outcome package JSON is invalid.");
        if (!string.Equals(package.Outcome.ArtifactFingerprint, artifactFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Research outcome identity does not match its storage key.");
        ResearchGateBundleRules.Validate(package.Launch.Gates);
        ResearchLaunchIntentRules.Validate(package.Launch.Intent, package.Launch.Gates);
        ResearchOutcomeArtifactRules.Validate(package.Outcome, package.Launch.Intent);
        return package;
    }

    private static string SafeSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Any(char.IsWhiteSpace) ||
            value.Any(ch => !(char.IsLetterOrDigit(ch) || ch is '-' or '_')))
            throw new ArgumentException("Research outcome fingerprint is not a safe storage identifier.", nameof(value));
        return value;
    }
}
