using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace QuantForge.Core;

public sealed record ResearchWorkspaceEntry(
    string EntryType,
    string Fingerprint,
    string DatasetFingerprint,
    string? StrategyFingerprint,
    string? JobFingerprint);

public sealed record ResearchWorkspaceIndex(
    string IndexFingerprint,
    IReadOnlyList<ResearchWorkspaceEntry> Entries);

public static class ResearchWorkspaceIndexRules
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.Ordinal)
    {
        "research-gates", "research-launch", "research-outcome", "research-comparison"
    };

    public static ResearchWorkspaceIndex Create(IReadOnlyList<ResearchWorkspaceEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        if (entries.Count == 0)
            throw new InvalidOperationException("Research workspace index requires at least one evidence reference.");

        var ordered = entries
            .OrderBy(x => x.EntryType, StringComparer.Ordinal)
            .ThenBy(x => x.Fingerprint, StringComparer.Ordinal)
            .ToArray();

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in ordered)
        {
            if (!AllowedTypes.Contains(entry.EntryType))
                throw new InvalidOperationException("Research workspace index contains an unsupported evidence type.");
            if (string.IsNullOrWhiteSpace(entry.Fingerprint) || string.IsNullOrWhiteSpace(entry.DatasetFingerprint))
                throw new InvalidOperationException("Research workspace evidence identity is incomplete.");
            if (!seen.Add(entry.EntryType + "|" + entry.Fingerprint))
                throw new InvalidOperationException("Research workspace index cannot contain duplicate evidence references.");
        }

        return new(Fingerprint(ordered), ordered);
    }

    public static void Validate(ResearchWorkspaceIndex index)
    {
        ArgumentNullException.ThrowIfNull(index);
        var expected = Create(index.Entries);
        if (!string.Equals(expected.IndexFingerprint, index.IndexFingerprint, StringComparison.Ordinal) ||
            !expected.Entries.SequenceEqual(index.Entries))
            throw new InvalidOperationException("Research workspace index is not canonical or its fingerprint is invalid.");
    }

    public static ResearchWorkspaceEntry From(ResearchGateBundle gates) =>
        new("research-gates", gates.BundleFingerprint, gates.Dataset.DatasetFingerprint,
            gates.Strategy.Envelope.Manifest.SourceFingerprint, null);

    public static ResearchWorkspaceEntry From(ResearchLaunchIntent launch) =>
        new("research-launch", launch.IntentFingerprint, launch.Job.Identity.DatasetFingerprint,
            launch.Job.Identity.StrategyFingerprint, launch.Job.Identity.JobFingerprint);

    public static ResearchWorkspaceEntry From(ResearchOutcomeArtifact outcome) =>
        new("research-outcome", outcome.ArtifactFingerprint, outcome.Report.DatasetFingerprint,
            outcome.Report.StrategyFingerprint, outcome.Report.JobId);

    public static ResearchWorkspaceEntry From(ResearchOutcomeComparison comparison) =>
        new("research-comparison", comparison.ComparisonFingerprint, comparison.DatasetFingerprint, null, null);

    private static string Fingerprint(IReadOnlyList<ResearchWorkspaceEntry> entries)
    {
        var text = string.Join("\n", entries.Select(x =>
            string.Join("|", x.EntryType, x.Fingerprint, x.DatasetFingerprint, x.StrategyFingerprint ?? string.Empty, x.JobFingerprint ?? string.Empty)));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
    }
}

public sealed class ResearchWorkspaceIndexFileStore
{
    private readonly string _path;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public ResearchWorkspaceIndexFileStore(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("A research workspace index path is required.", nameof(path));
        _path = Path.GetFullPath(path);
    }

    public void Save(ResearchWorkspaceIndex index)
    {
        ResearchWorkspaceIndexRules.Validate(index);
        var directory = Path.GetDirectoryName(_path) ?? throw new InvalidOperationException("Research workspace index path is invalid.");
        Directory.CreateDirectory(directory);
        var json = JsonSerializer.Serialize(index, JsonOptions);
        var temp = _path + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            File.WriteAllText(temp, json, new UTF8Encoding(false));
            File.Move(temp, _path, true);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }

    public ResearchWorkspaceIndex Load()
    {
        if (!File.Exists(_path))
            throw new FileNotFoundException("Research workspace index does not exist.", _path);
        var index = JsonSerializer.Deserialize<ResearchWorkspaceIndex>(File.ReadAllText(_path, Encoding.UTF8), JsonOptions)
            ?? throw new InvalidOperationException("Research workspace index JSON is invalid.");
        ResearchWorkspaceIndexRules.Validate(index);
        return index;
    }
}
