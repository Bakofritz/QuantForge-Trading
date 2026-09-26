using System.Text;
using System.Text.Json;

namespace QuantForge.Core;

public sealed class ResearchWorkflowSummaryFileStore
{
    private readonly string _rootDirectory;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public ResearchWorkflowSummaryFileStore(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("A workflow summary directory is required.", nameof(rootDirectory));
        _rootDirectory = Path.GetFullPath(rootDirectory);
    }

    public string Save(ResearchWorkflowSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);
        var markdown = ResearchWorkflowMarkdownExporter.Export(summary);
        var directory = Path.Combine(_rootDirectory, SafeSegment(summary.WorkflowFingerprint));
        Directory.CreateDirectory(_rootDirectory);
        var path = Path.Combine(directory, "report.md");
        var jsonPath = Path.Combine(directory, "summary.json");

        if (Directory.Exists(directory))
        {
            if (!File.Exists(path) || !File.Exists(jsonPath))
                throw new InvalidOperationException("Workflow summary directory is incomplete.");
            if (!string.Equals(File.ReadAllText(path, Encoding.UTF8), markdown, StringComparison.Ordinal))
                throw new InvalidOperationException("Workflow fingerprint collision contains different report content.");
            var existing = JsonSerializer.Deserialize<ResearchWorkflowSummary>(File.ReadAllText(jsonPath, Encoding.UTF8), JsonOptions)
                ?? throw new InvalidOperationException("Workflow summary JSON is invalid.");
            if (existing.WorkflowFingerprint != summary.WorkflowFingerprint)
                throw new InvalidOperationException("Workflow summary identity mismatch.");
            return path;
        }

        var temp = directory + ".tmp-" + Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(temp);
        try
        {
            File.WriteAllText(Path.Combine(temp, "report.md"), markdown, new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(temp, "summary.json"), JsonSerializer.Serialize(summary, JsonOptions), new UTF8Encoding(false));
            Directory.Move(temp, directory);
            return path;
        }
        catch
        {
            if (Directory.Exists(temp)) Directory.Delete(temp, true);
            throw;
        }
    }

    public ResearchWorkflowSummary Load(string workflowFingerprint)
    {
        var directory = Path.Combine(_rootDirectory, SafeSegment(workflowFingerprint));
        var jsonPath = Path.Combine(directory, "summary.json");
        if (!File.Exists(jsonPath))
            throw new FileNotFoundException("Workflow summary does not exist.", jsonPath);
        var summary = JsonSerializer.Deserialize<ResearchWorkflowSummary>(File.ReadAllText(jsonPath, Encoding.UTF8), JsonOptions)
            ?? throw new InvalidOperationException("Workflow summary JSON is invalid.");
        if (!string.Equals(summary.WorkflowFingerprint, workflowFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Workflow summary fingerprint does not match its storage key.");
        var rebuilt = ResearchWorkflowSummaryFactory.Create(summary.Mode, new ResearchWorkflowResult(
            summary.Reports,
            summary.Reports.Select(ResearchComponentStatusRules.FromReport).ToArray(),
            summary.Reliability));
        if (!string.Equals(rebuilt.WorkflowFingerprint, summary.WorkflowFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Workflow summary fingerprint verification failed.");
        return summary;
    }

    private static string SafeSegment(string fingerprint)
    {
        if (string.IsNullOrWhiteSpace(fingerprint) || fingerprint.Any(char.IsWhiteSpace) ||
            fingerprint.Any(ch => !(char.IsLetterOrDigit(ch) || ch == '-' || ch == '_')))
            throw new ArgumentException("Workflow fingerprint is not a safe storage identifier.", nameof(fingerprint));
        return fingerprint;
    }
}
