using System.Text;
using System.Text.Json;

namespace QuantForge.Core;

public sealed class ResearchOptimizationFileStore
{
    private readonly string _rootDirectory;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public ResearchOptimizationFileStore(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("An optimization store directory is required.", nameof(rootDirectory));
        _rootDirectory = Path.GetFullPath(rootDirectory);
    }

    public string Save(ResearchOptimizationApplicationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        var state = ProductUiOptimizationResultPresenter.Create(result);
        var directory = Path.Combine(_rootDirectory, SafeSegment(state.ResultFingerprint));
        var text = ExportText(state);
        var json = JsonSerializer.Serialize(state, JsonOptions);
        Directory.CreateDirectory(_rootDirectory);
        var textPath = Path.Combine(directory, "optimization.txt");
        var jsonPath = Path.Combine(directory, "optimization.json");

        if (Directory.Exists(directory))
        {
            if (!File.Exists(textPath) || !File.Exists(jsonPath))
                throw new InvalidOperationException("Optimization result directory is incomplete.");
            if (!string.Equals(File.ReadAllText(textPath, Encoding.UTF8), text, StringComparison.Ordinal) ||
                !string.Equals(File.ReadAllText(jsonPath, Encoding.UTF8), json, StringComparison.Ordinal))
                throw new InvalidOperationException("Optimization fingerprint collision contains different content.");
            return textPath;
        }

        var temp = directory + ".tmp-" + Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(temp);
        try
        {
            File.WriteAllText(Path.Combine(temp, "optimization.txt"), text, new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(temp, "optimization.json"), json, new UTF8Encoding(false));
            Directory.Move(temp, directory);
            return textPath;
        }
        catch
        {
            if (Directory.Exists(temp)) Directory.Delete(temp, true);
            throw;
        }
    }

    public ProductUiOptimizationResultState Load(string resultFingerprint)
    {
        var directory = Path.Combine(_rootDirectory, SafeSegment(resultFingerprint));
        var textPath = Path.Combine(directory, "optimization.txt");
        var jsonPath = Path.Combine(directory, "optimization.json");
        if (!File.Exists(textPath) || !File.Exists(jsonPath))
            throw new FileNotFoundException("Optimization result is incomplete or does not exist.", directory);

        var state = JsonSerializer.Deserialize<ProductUiOptimizationResultState>(
            File.ReadAllText(jsonPath, Encoding.UTF8), JsonOptions)
            ?? throw new InvalidOperationException("Optimization result JSON is invalid.");
        ValidateRecovered(state, resultFingerprint);
        if (!string.Equals(File.ReadAllText(textPath, Encoding.UTF8), ExportText(state), StringComparison.Ordinal))
            throw new InvalidOperationException("Optimization text representation does not match recovered JSON state.");
        return state;
    }

    private static void ValidateRecovered(ProductUiOptimizationResultState state, string storageKey)
    {
        if (!string.Equals(state.ResultFingerprint, storageKey, StringComparison.Ordinal))
            throw new InvalidOperationException("Optimization result identity does not match its storage key.");
        if (state.LiveAccountEnabled || state.CanSubmitOrders || state.CanChangeApplicationSettings)
            throw new InvalidOperationException("Recovered optimization state cannot acquire live, order, or settings authority.");
        if (state.Variants is null)
            throw new InvalidOperationException("Recovered optimization state requires its complete variant collection.");
        if (state.Variants.GroupBy(x => x.JobId, StringComparer.Ordinal).Any(x => x.Count() > 1))
            throw new InvalidOperationException("Recovered optimization state contains duplicate job identities.");

        if (state.Complete)
        {
            if (state.Objective is null || string.IsNullOrWhiteSpace(state.SelectedJobId) || state.Variants.Count == 0)
                throw new InvalidOperationException("Complete recovered optimization state requires an objective and selected variant.");
            if (state.Variants.Any(x => x.Status != ResearchResultStatus.Complete || x.FinalEquity is null || x.RealizedPnl is null))
                throw new InvalidOperationException("Complete recovered optimization state requires complete simulated metrics for every variant.");
            var selected = state.Variants.Where(x => x.Selected).ToArray();
            if (selected.Length != 1 || !string.Equals(selected[0].JobId, state.SelectedJobId, StringComparison.Ordinal))
                throw new InvalidOperationException("Recovered optimization selection is inconsistent.");
        }
        else
        {
            if (state.Objective is not null || state.SelectedJobId is not null || state.Variants.Any(x => x.Selected))
                throw new InvalidOperationException("Blocked recovered optimization state cannot contain a selection.");
            if (state.Variants.Any(x => x.Status != ResearchResultStatus.Complete &&
                                        (x.FinalEquity is not null || x.RealizedPnl is not null)))
                throw new InvalidOperationException("Blocked or invalid recovered variants cannot contain simulated performance state.");
        }
    }

    private static string ExportText(ProductUiOptimizationResultState state)
    {
        var rows = state.Variants
            .OrderBy(x => x.JobId, StringComparer.Ordinal)
            .Select(x => string.Join("|", new[]
            {
                x.JobId,
                x.Status.ToString(),
                x.FinalEquity?.ToString("G29", System.Globalization.CultureInfo.InvariantCulture) ?? "n/a",
                x.RealizedPnl?.ToString("G29", System.Globalization.CultureInfo.InvariantCulture) ?? "n/a",
                x.Selected ? "selected" : "not-selected",
                x.Message ?? "n/a"
            }));
        return string.Join("\n", new[]
        {
            "quantforge-optimization-v2",
            $"result={state.ResultFingerprint}",
            $"complete={state.Complete}",
            $"selection={(state.Complete ? "verified" : "blocked")}",
            $"selected-job={state.SelectedJobId ?? "n/a"}",
            $"objective={state.Objective?.ToString() ?? "n/a"}",
            string.Join("\n", rows)
        }) + "\n";
    }

    private static string SafeSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Any(char.IsWhiteSpace) || value.Any(ch => !(char.IsLetterOrDigit(ch) || ch is '-' or '_')))
            throw new ArgumentException("Optimization result fingerprint is not a safe storage identifier.", nameof(value));
        return value;
    }
}

public static class ResearchOptimizationRecovery
{
    public static ProductUiOptimizationResultState Load(string rootDirectory, string resultFingerprint) =>
        new ResearchOptimizationFileStore(rootDirectory).Load(resultFingerprint);
}
