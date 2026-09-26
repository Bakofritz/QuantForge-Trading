using System.Text;
using System.Text.Json;

namespace QuantForge.Core;

public sealed record ResearchOutcomeComparisonPackage(
    ResearchOutcomePackage Left,
    ResearchOutcomePackage Right,
    ResearchOutcomeComparison Comparison);

public sealed class ResearchOutcomeComparisonFileStore
{
    private readonly string _rootDirectory;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public ResearchOutcomeComparisonFileStore(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("A research outcome comparison directory is required.", nameof(rootDirectory));
        _rootDirectory = Path.GetFullPath(rootDirectory);
    }

    public string Save(ResearchOutcomeComparisonPackage package)
    {
        ArgumentNullException.ThrowIfNull(package);
        ResearchOutcomeComparisonRules.Validate(package.Comparison, package.Left, package.Right);
        var directory = Path.Combine(_rootDirectory, SafeSegment(package.Comparison.ComparisonFingerprint));
        var path = Path.Combine(directory, "research-comparison.json");
        var json = JsonSerializer.Serialize(package, JsonOptions);
        Directory.CreateDirectory(_rootDirectory);
        if (Directory.Exists(directory))
        {
            if (!File.Exists(path)) throw new InvalidOperationException("Research comparison directory is incomplete.");
            if (!string.Equals(File.ReadAllText(path, Encoding.UTF8), json, StringComparison.Ordinal))
                throw new InvalidOperationException("Research comparison fingerprint collision contains different content.");
            return path;
        }
        var temp = directory + ".tmp-" + Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(temp);
        try
        {
            File.WriteAllText(Path.Combine(temp, "research-comparison.json"), json, new UTF8Encoding(false));
            Directory.Move(temp, directory);
            return path;
        }
        catch
        {
            if (Directory.Exists(temp)) Directory.Delete(temp, true);
            throw;
        }
    }

    public ResearchOutcomeComparisonPackage Load(string comparisonFingerprint)
    {
        var path = Path.Combine(_rootDirectory, SafeSegment(comparisonFingerprint), "research-comparison.json");
        if (!File.Exists(path)) throw new FileNotFoundException("Research comparison package does not exist.", path);
        var package = JsonSerializer.Deserialize<ResearchOutcomeComparisonPackage>(File.ReadAllText(path, Encoding.UTF8), JsonOptions)
            ?? throw new InvalidOperationException("Research comparison package JSON is invalid.");
        if (!string.Equals(package.Comparison.ComparisonFingerprint, comparisonFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Research comparison identity does not match its storage key.");
        ResearchOutcomeComparisonRules.Validate(package.Comparison, package.Left, package.Right);
        return package;
    }

    private static string SafeSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Any(char.IsWhiteSpace) ||
            value.Any(ch => !(char.IsLetterOrDigit(ch) || ch is '-' or '_')))
            throw new ArgumentException("Research comparison fingerprint is not a safe storage identifier.", nameof(value));
        return value;
    }
}
