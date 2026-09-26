using System.Text;
using System.Text.Json;

namespace QuantForge.Core;

public sealed class WalkForwardResearchFileStore
{
    private readonly string _rootDirectory;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public WalkForwardResearchFileStore(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("A walk-forward result directory is required.", nameof(rootDirectory));
        _rootDirectory = Path.GetFullPath(rootDirectory);
    }

    public string Save(WalkForwardResearchResult result)
    {
        WalkForwardResearchResultRules.Validate(result);
        var directory = Path.Combine(_rootDirectory, SafeSegment(result.ResultFingerprint));
        var json = JsonSerializer.Serialize(result, JsonOptions);
        var markdown = ExportMarkdown(ProductUiWalkForwardPresenter.Create(result));
        var jsonPath = Path.Combine(directory, "walk-forward.json");
        var markdownPath = Path.Combine(directory, "walk-forward.md");
        Directory.CreateDirectory(_rootDirectory);

        if (Directory.Exists(directory))
        {
            if (!File.Exists(jsonPath) || !File.Exists(markdownPath))
                throw new InvalidOperationException("Walk-forward result directory is incomplete.");
            if (!string.Equals(File.ReadAllText(jsonPath, Encoding.UTF8), json, StringComparison.Ordinal) ||
                !string.Equals(File.ReadAllText(markdownPath, Encoding.UTF8), markdown, StringComparison.Ordinal))
                throw new InvalidOperationException("Walk-forward result fingerprint collision contains different content.");
            return jsonPath;
        }

        var temp = directory + ".tmp-" + Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(temp);
        try
        {
            File.WriteAllText(Path.Combine(temp, "walk-forward.json"), json, new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(temp, "walk-forward.md"), markdown, new UTF8Encoding(false));
            Directory.Move(temp, directory);
            return jsonPath;
        }
        catch
        {
            if (Directory.Exists(temp)) Directory.Delete(temp, true);
            throw;
        }
    }

    public WalkForwardResearchResult Load(string resultFingerprint)
    {
        var directory = Path.Combine(_rootDirectory, SafeSegment(resultFingerprint));
        var jsonPath = Path.Combine(directory, "walk-forward.json");
        var markdownPath = Path.Combine(directory, "walk-forward.md");
        if (!File.Exists(jsonPath) || !File.Exists(markdownPath))
            throw new FileNotFoundException("Walk-forward result is incomplete or does not exist.", directory);

        var result = JsonSerializer.Deserialize<WalkForwardResearchResult>(File.ReadAllText(jsonPath, Encoding.UTF8), JsonOptions)
            ?? throw new InvalidOperationException("Walk-forward result JSON is invalid.");
        if (!string.Equals(result.ResultFingerprint, resultFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Walk-forward result identity does not match its storage key.");
        WalkForwardResearchResultRules.Validate(result);
        var expectedMarkdown = ExportMarkdown(ProductUiWalkForwardPresenter.Create(result));
        if (!string.Equals(File.ReadAllText(markdownPath, Encoding.UTF8), expectedMarkdown, StringComparison.Ordinal))
            throw new InvalidOperationException("Walk-forward Markdown does not match the recovered JSON result.");
        return result;
    }

    private static string ExportMarkdown(ProductUiWalkForwardState state)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# QuantForge Walk-Forward Result");
        builder.AppendLine($"- Result: `{state.ResultFingerprint}`");
        builder.AppendLine($"- Complete: `{state.Complete}`");
        builder.AppendLine($"- Block reason: {state.BlockReason ?? "n/a"}");
        builder.AppendLine();
        builder.AppendLine("| Segment | Training selection | Evaluation | Equity | Realized P&L | Segment fingerprint |");
        builder.AppendLine("| --- | --- | --- | ---: | ---: | --- |");
        foreach (var segment in state.Segments)
        {
            builder.Append("| ").Append(Escape(segment.SegmentId)).Append(" | ")
                .Append(Escape(segment.SelectedTrainingJobId)).Append(" | ")
                .Append(Escape(segment.EvaluationStatus?.ToString())).Append(" | ")
                .Append(segment.FinalEquity?.ToString("G29", System.Globalization.CultureInfo.InvariantCulture) ?? "n/a").Append(" | ")
                .Append(segment.RealizedPnl?.ToString("G29", System.Globalization.CultureInfo.InvariantCulture) ?? "n/a").Append(" | ")
                .Append(Escape(segment.SegmentFingerprint)).AppendLine(" |");
        }
        return builder.ToString();
    }

    private static string Escape(string? value) => (value ?? "n/a").Replace("|", "\\|", StringComparison.Ordinal);

    private static string SafeSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Any(char.IsWhiteSpace) ||
            value.Any(ch => !(char.IsLetterOrDigit(ch) || ch is '-' or '_')))
            throw new ArgumentException("Walk-forward result fingerprint is not a safe storage identifier.", nameof(value));
        return value;
    }
}
