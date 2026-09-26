using System.Text;

namespace QuantForge.Core;

public sealed class ResearchPublicationFileStore
{
    private readonly string _rootDirectory;

    public ResearchPublicationFileStore(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("A publication store directory is required.", nameof(rootDirectory));
        _rootDirectory = Path.GetFullPath(rootDirectory);
    }

    public string Save(ResearchPublicationArtifact artifact)
    {
        ResearchPublicationArtifactRules.Validate(artifact);
        var directory = Path.Combine(_rootDirectory, SafeSegment(artifact.ArtifactFingerprint));
        Directory.CreateDirectory(_rootDirectory);

        var manifestPath = Path.Combine(directory, "manifest.txt");
        if (Directory.Exists(directory))
        {
            if (!File.Exists(manifestPath))
                throw new InvalidOperationException("Publication directory exists without a manifest.");
            var existing = File.ReadAllText(manifestPath, Encoding.UTF8);
            if (!string.Equals(existing, artifact.Manifest, StringComparison.Ordinal))
                throw new InvalidOperationException("Publication fingerprint collision contains different content.");
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
            if (Directory.Exists(temp))
                Directory.Delete(temp, recursive: true);
            throw;
        }
    }

    public bool Exists(string artifactFingerprint) =>
        Directory.Exists(Path.Combine(_rootDirectory, SafeSegment(artifactFingerprint))) &&
        File.Exists(Path.Combine(_rootDirectory, SafeSegment(artifactFingerprint), "manifest.txt"));

    private static string SafeSegment(string fingerprint)
    {
        if (string.IsNullOrWhiteSpace(fingerprint) || fingerprint.Any(char.IsWhiteSpace) ||
            fingerprint.Any(ch => !(char.IsLetterOrDigit(ch) || ch == '-' || ch == '_')))
            throw new ArgumentException("Artifact fingerprint is not a safe storage identifier.", nameof(fingerprint));
        return fingerprint;
    }
}
