using System.Text;
using System.Text.Json;

namespace QuantForge.Core;

public sealed class ResearchReportFileStore
{
    private readonly string _rootDirectory;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public ResearchReportFileStore(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("A research report directory is required.", nameof(rootDirectory));
        _rootDirectory = Path.GetFullPath(rootDirectory);
    }

    public string Save(ResearchReport report)
    {
        ResearchReportRules.Validate(report);
        var directory = Path.Combine(_rootDirectory, SafeSegment(report.JobId));
        Directory.CreateDirectory(_rootDirectory);
        var jsonPath = Path.Combine(directory, "report.json");
        var markdownPath = Path.Combine(directory, "report.md");
        var json = JsonSerializer.Serialize(report, JsonOptions);
        var markdown = ExportMarkdown(report);

        if (Directory.Exists(directory))
        {
            if (!File.Exists(jsonPath) || !File.Exists(markdownPath))
                throw new InvalidOperationException("Research report directory is incomplete.");
            if (!string.Equals(File.ReadAllText(jsonPath, Encoding.UTF8), json, StringComparison.Ordinal) ||
                !string.Equals(File.ReadAllText(markdownPath, Encoding.UTF8), markdown, StringComparison.Ordinal))
                throw new InvalidOperationException("Research report identity collision contains different content.");
            return jsonPath;
        }

        var temp = directory + ".tmp-" + Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(temp);
        try
        {
            File.WriteAllText(Path.Combine(temp, "report.json"), json, new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(temp, "report.md"), markdown, new UTF8Encoding(false));
            Directory.Move(temp, directory);
            return jsonPath;
        }
        catch
        {
            if (Directory.Exists(temp)) Directory.Delete(temp, true);
            throw;
        }
    }

    public ResearchReport Load(string jobId)
    {
        var path = Path.Combine(_rootDirectory, SafeSegment(jobId), "report.json");
        if (!File.Exists(path)) throw new FileNotFoundException("Research report does not exist.", path);
        var report = JsonSerializer.Deserialize<ResearchReport>(File.ReadAllText(path, Encoding.UTF8), JsonOptions)
            ?? throw new InvalidOperationException("Research report JSON is invalid.");
        ResearchReportRules.Validate(report);
        if (!string.Equals(report.JobId, jobId, StringComparison.Ordinal))
            throw new InvalidOperationException("Research report storage key does not match job identity.");
        var markdownPath = Path.Combine(_rootDirectory, SafeSegment(jobId), "report.md");
        if (!File.Exists(markdownPath) ||
            !string.Equals(File.ReadAllText(markdownPath, Encoding.UTF8), ProductApplicationReportExport.ExportReport(report), StringComparison.Ordinal))
            throw new InvalidOperationException("Research report Markdown does not match its recovered JSON identity.");
        return report;
    }

    private static string ExportMarkdown(ResearchReport report) =>
        string.Join("\n", new[]
        {
            "# QuantForge Research Report",
            $"- Job: `{report.JobId}`",
            $"- Status: `{report.Status}`",
            $"- Dataset: `{report.DatasetFingerprint}`",
            $"- Strategy: `{report.StrategyFingerprint}`",
            $"- Execution policy: `{report.ExecutionPolicyFingerprint}`",
            $"- Parameters: `{report.ParameterFingerprint}`",
            $"- Partition: `{report.TemporalPartition}`",
            $"- Evidence: `{report.EvidenceTail?.Fingerprint ?? "n/a"}`",
            $"- Reason: {report.BlockReason ?? "n/a"}"
        }) + "\n";

    private static string SafeSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Any(char.IsWhiteSpace) ||
            value.Any(ch => !(char.IsLetterOrDigit(ch) || ch == '-' || ch == '_')))
            throw new ArgumentException("Job identity is not a safe storage identifier.", nameof(value));
        return value;
    }
}
