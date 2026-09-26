using System.Text;

namespace QuantForge.Core;

/// <summary>
/// Durable result archive. Files are keyed by the immutable artifact fingerprint;
/// existing content must match exactly before a repeat publication is accepted.
/// </summary>
public sealed class ResearchResultArchiveFileStore
{
    private readonly string _rootDirectory;

    public ResearchResultArchiveFileStore(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("A result archive directory is required.", nameof(rootDirectory));
        _rootDirectory = Path.GetFullPath(rootDirectory);
    }

    public string Save(ResearchPublicationArtifact artifact)
    {
        ArgumentNullException.ThrowIfNull(artifact);
        ResearchPublicationArtifactRules.Validate(artifact);

        var archive = new ResearchResultArchive();
        archive.Add(artifact);

        Directory.CreateDirectory(_rootDirectory);
        var directory = Path.Combine(_rootDirectory, SafeSegment(artifact.ArtifactFingerprint));
        if (Directory.Exists(directory))
        {
            var existingManifest = Path.Combine(directory, "manifest.txt");
            if (!File.Exists(existingManifest))
                throw new InvalidOperationException("Result archive entry exists without a manifest.");
            if (!string.Equals(File.ReadAllText(existingManifest, Encoding.UTF8), artifact.Manifest, StringComparison.Ordinal))
                throw new InvalidOperationException("Result archive fingerprint collision contains different content.");
            return directory;
        }

        var temp = directory + ".tmp-" + Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(temp);
        try
        {
            File.WriteAllText(Path.Combine(temp, "manifest.txt"), artifact.Manifest, new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(temp, "chart.csv"), artifact.ChartData, new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(temp, "ledger.csv"), artifact.LedgerData, new UTF8Encoding(false));
            Directory.Move(temp, directory);
            return directory;
        }
        catch
        {
            if (Directory.Exists(temp)) Directory.Delete(temp, recursive: true);
            throw;
        }
    }

    public void VerifyStored(string artifactFingerprint)
    {
        var directory = Path.Combine(_rootDirectory, SafeSegment(artifactFingerprint));
        if (!Directory.Exists(directory))
            throw new FileNotFoundException("Research result archive entry does not exist.", directory);

        var manifestPath = Path.Combine(directory, "manifest.txt");
        var chartPath = Path.Combine(directory, "chart.csv");
        var ledgerPath = Path.Combine(directory, "ledger.csv");
        if (!File.Exists(manifestPath) || !File.Exists(chartPath) || !File.Exists(ledgerPath))
            throw new InvalidOperationException("Research result archive entry is incomplete.");

        var manifest = File.ReadAllText(manifestPath, Encoding.UTF8);
        var chartHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(chartPath)));
        var ledgerHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(ledgerPath)));
        var expectedChart = ManifestValue(manifest, "chart-sha256");
        var expectedLedger = ManifestValue(manifest, "ledger-sha256");
        if (!string.Equals(expectedChart, chartHash, StringComparison.Ordinal) ||
            !string.Equals(expectedLedger, ledgerHash, StringComparison.Ordinal))
            throw new InvalidOperationException("Research result archive artifact content does not match its manifest.");
    }

    public bool Exists(string artifactFingerprint) =>
        Directory.Exists(Path.Combine(_rootDirectory, SafeSegment(artifactFingerprint))) &&
        File.Exists(Path.Combine(_rootDirectory, SafeSegment(artifactFingerprint), "manifest.txt"));

    private static string ManifestValue(string manifest, string key)
    {
        var prefix = key + "=";
        var line = manifest.Split('\n').FirstOrDefault(x => x.StartsWith(prefix, StringComparison.Ordinal));
        if (line is null)
            throw new InvalidOperationException($"Research result archive manifest is missing {key}.");
        return line[prefix.Length..].Trim();
    }

    private static string SafeSegment(string fingerprint)
    {
        if (string.IsNullOrWhiteSpace(fingerprint) || fingerprint.Any(char.IsWhiteSpace) ||
            fingerprint.Any(ch => !(char.IsLetterOrDigit(ch) || ch == '-' || ch == '_')))
            throw new ArgumentException("Artifact fingerprint is not a safe storage identifier.", nameof(fingerprint));
        return fingerprint;
    }
}
