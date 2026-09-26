using System.Text;
using System.Text.Json;

namespace QuantForge.Core;

public sealed class ResearchGateBundleFileStore
{
    private readonly string _rootDirectory;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public ResearchGateBundleFileStore(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("A research gate bundle directory is required.", nameof(rootDirectory));
        _rootDirectory = Path.GetFullPath(rootDirectory);
    }

    public string Save(ResearchGateBundle bundle)
    {
        ResearchGateBundleRules.Validate(bundle);
        var directory = Path.Combine(_rootDirectory, SafeSegment(bundle.BundleFingerprint));
        var path = Path.Combine(directory, "research-gates.json");
        var json = JsonSerializer.Serialize(bundle, JsonOptions);
        Directory.CreateDirectory(_rootDirectory);

        if (Directory.Exists(directory))
        {
            if (!File.Exists(path)) throw new InvalidOperationException("Research gate bundle directory is incomplete.");
            if (!string.Equals(File.ReadAllText(path, Encoding.UTF8), json, StringComparison.Ordinal))
                throw new InvalidOperationException("Research gate bundle fingerprint collision contains different content.");
            return path;
        }

        var temp = directory + ".tmp-" + Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(temp);
        try
        {
            File.WriteAllText(Path.Combine(temp, "research-gates.json"), json, new UTF8Encoding(false));
            Directory.Move(temp, directory);
            return path;
        }
        catch
        {
            if (Directory.Exists(temp)) Directory.Delete(temp, true);
            throw;
        }
    }

    public ResearchGateBundle Load(string bundleFingerprint)
    {
        var path = Path.Combine(_rootDirectory, SafeSegment(bundleFingerprint), "research-gates.json");
        if (!File.Exists(path)) throw new FileNotFoundException("Research gate bundle does not exist.", path);
        var bundle = JsonSerializer.Deserialize<ResearchGateBundle>(File.ReadAllText(path, Encoding.UTF8), JsonOptions)
            ?? throw new InvalidOperationException("Research gate bundle JSON is invalid.");
        if (!string.Equals(bundle.BundleFingerprint, bundleFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Research gate bundle identity does not match its storage key.");
        ResearchGateBundleRules.Validate(bundle);
        return bundle;
    }

    private static string SafeSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Any(char.IsWhiteSpace) ||
            value.Any(ch => !(char.IsLetterOrDigit(ch) || ch is '-' or '_')))
            throw new ArgumentException("Research gate fingerprint is not a safe storage identifier.", nameof(value));
        return value;
    }
}
