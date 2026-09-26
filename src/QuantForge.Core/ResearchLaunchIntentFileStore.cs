using System.Text;
using System.Text.Json;

namespace QuantForge.Core;

public sealed record ResearchLaunchPackage(
    ResearchGateBundle Gates,
    ResearchLaunchIntent Intent);

public sealed class ResearchLaunchIntentFileStore
{
    private readonly string _rootDirectory;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public ResearchLaunchIntentFileStore(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("A research launch directory is required.", nameof(rootDirectory));
        _rootDirectory = Path.GetFullPath(rootDirectory);
    }

    public string Save(ResearchLaunchPackage package)
    {
        ArgumentNullException.ThrowIfNull(package);
        ResearchGateBundleRules.Validate(package.Gates);
        ResearchLaunchIntentRules.Validate(package.Intent, package.Gates);
        var directory = Path.Combine(_rootDirectory, SafeSegment(package.Intent.IntentFingerprint));
        var path = Path.Combine(directory, "research-launch.json");
        var json = JsonSerializer.Serialize(package, JsonOptions);
        Directory.CreateDirectory(_rootDirectory);
        if (Directory.Exists(directory))
        {
            if (!File.Exists(path)) throw new InvalidOperationException("Research launch directory is incomplete.");
            if (!string.Equals(File.ReadAllText(path, Encoding.UTF8), json, StringComparison.Ordinal))
                throw new InvalidOperationException("Research launch fingerprint collision contains different content.");
            return path;
        }
        var temp = directory + ".tmp-" + Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(temp);
        try
        {
            File.WriteAllText(Path.Combine(temp, "research-launch.json"), json, new UTF8Encoding(false));
            Directory.Move(temp, directory);
            return path;
        }
        catch
        {
            if (Directory.Exists(temp)) Directory.Delete(temp, true);
            throw;
        }
    }

    public ResearchLaunchPackage Load(string intentFingerprint)
    {
        var path = Path.Combine(_rootDirectory, SafeSegment(intentFingerprint), "research-launch.json");
        if (!File.Exists(path)) throw new FileNotFoundException("Research launch package does not exist.", path);
        var package = JsonSerializer.Deserialize<ResearchLaunchPackage>(File.ReadAllText(path, Encoding.UTF8), JsonOptions)
            ?? throw new InvalidOperationException("Research launch package JSON is invalid.");
        if (!string.Equals(package.Intent.IntentFingerprint, intentFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Research launch identity does not match its storage key.");
        ResearchGateBundleRules.Validate(package.Gates);
        ResearchLaunchIntentRules.Validate(package.Intent, package.Gates);
        return package;
    }

    private static string SafeSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Any(char.IsWhiteSpace) ||
            value.Any(ch => !(char.IsLetterOrDigit(ch) || ch is '-' or '_')))
            throw new ArgumentException("Research launch fingerprint is not a safe storage identifier.", nameof(value));
        return value;
    }
}
