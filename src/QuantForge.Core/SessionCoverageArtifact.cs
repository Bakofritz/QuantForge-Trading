using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace QuantForge.Core;

public sealed record SessionCoverageArtifact(
    string ArtifactFingerprint,
    SessionCoverageReport Report);

public static class SessionCoverageArtifactRules
{
    public static SessionCoverageArtifact Create(SessionCoverageReport report)
    {
        SessionCoverageRules.RequireResearchAdmissible(report);
        return new(Fingerprint(report), report);
    }

    public static void Validate(SessionCoverageArtifact artifact)
    {
        ArgumentNullException.ThrowIfNull(artifact);
        SessionCoverageRules.RequireResearchAdmissible(artifact.Report);
        var expected = Fingerprint(artifact.Report);
        if (!string.Equals(expected, artifact.ArtifactFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Session coverage artifact fingerprint does not match its report.");
    }

    private static string Fingerprint(SessionCoverageReport report)
    {
        var builder = new StringBuilder()
            .Append(report.DatasetFingerprint).Append('|')
            .Append(report.PolicyFingerprint).Append('|')
            .Append(report.ExpectedMinuteCount.ToString(CultureInfo.InvariantCulture)).Append('|')
            .Append(report.ObservedInSessionMinuteCount.ToString(CultureInfo.InvariantCulture)).Append('|')
            .Append(report.MissingMinuteCount.ToString(CultureInfo.InvariantCulture)).Append('|')
            .Append(report.OutsideSessionMinuteCount.ToString(CultureInfo.InvariantCulture)).Append('|')
            .Append(report.Authoritative).Append('|')
            .Append(report.Limitation).Append('\n');
        foreach (var stamp in report.MissingMinutes.OrderBy(x => x))
            builder.Append("missing|").Append(stamp.ToUniversalTime().ToString("O")).Append('\n');
        foreach (var stamp in report.OutsideSessionMinutes.OrderBy(x => x))
            builder.Append("outside|").Append(stamp.ToUniversalTime().ToString("O")).Append('\n');
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString())));
    }
}

public sealed class SessionCoverageArtifactFileStore
{
    private readonly string _rootDirectory;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public SessionCoverageArtifactFileStore(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("A session coverage evidence directory is required.", nameof(rootDirectory));
        _rootDirectory = Path.GetFullPath(rootDirectory);
    }

    public string Save(SessionCoverageArtifact artifact)
    {
        SessionCoverageArtifactRules.Validate(artifact);
        var directory = Path.Combine(_rootDirectory, SafeSegment(artifact.ArtifactFingerprint));
        var path = Path.Combine(directory, "session-coverage.json");
        var json = JsonSerializer.Serialize(artifact, JsonOptions);
        Directory.CreateDirectory(_rootDirectory);

        if (Directory.Exists(directory))
        {
            if (!File.Exists(path))
                throw new InvalidOperationException("Session coverage artifact directory is incomplete.");
            if (!string.Equals(File.ReadAllText(path, Encoding.UTF8), json, StringComparison.Ordinal))
                throw new InvalidOperationException("Session coverage artifact fingerprint collision contains different content.");
            return path;
        }

        var temp = directory + ".tmp-" + Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(temp);
        try
        {
            File.WriteAllText(Path.Combine(temp, "session-coverage.json"), json, new UTF8Encoding(false));
            Directory.Move(temp, directory);
            return path;
        }
        catch
        {
            if (Directory.Exists(temp)) Directory.Delete(temp, true);
            throw;
        }
    }

    public SessionCoverageArtifact Load(string artifactFingerprint)
    {
        var directory = Path.Combine(_rootDirectory, SafeSegment(artifactFingerprint));
        var path = Path.Combine(directory, "session-coverage.json");
        if (!File.Exists(path))
            throw new FileNotFoundException("Session coverage artifact does not exist.", path);
        var artifact = JsonSerializer.Deserialize<SessionCoverageArtifact>(File.ReadAllText(path, Encoding.UTF8), JsonOptions)
            ?? throw new InvalidOperationException("Session coverage artifact JSON is invalid.");
        if (!string.Equals(artifact.ArtifactFingerprint, artifactFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Session coverage artifact identity does not match its storage key.");
        SessionCoverageArtifactRules.Validate(artifact);
        return artifact;
    }

    private static string SafeSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Any(char.IsWhiteSpace) ||
            value.Any(ch => !(char.IsLetterOrDigit(ch) || ch is '-' or '_')))
            throw new ArgumentException("Session coverage fingerprint is not a safe storage identifier.", nameof(value));
        return value;
    }
}
