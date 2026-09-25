using System.Reflection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using QuantForge.Core;

namespace QuantForge.App;

internal static class AppDiagnostics
{
    internal static DiagnosticJournal? Current { get; set; }
    internal static string DirectoryPath => Path.Combine(FileSystem.AppDataDirectory, "diagnostics-v1");
    internal static void Record(DiagnosticAction action, DiagnosticOutcome outcome = DiagnosticOutcome.Observed, long duration = 0) =>
        Current?.Record(action, outcome, duration);
}

internal sealed class DiagnosticPanel : ContentView
{
    private readonly Label _status = new() { Text = "Diagnostics OFF. No automatic upload. Start replaces the previous session." };
    private readonly Button _start = new() { Text = "Start test session" };
    private readonly Button _stop = new() { Text = "Stop session" };
    private readonly Button _mark = new() { Text = "Mark problem" };
    private readonly Button _export = new() { Text = "Export diagnostics ZIP" };
    private readonly Button _clear = new() { Text = "Clear diagnostics" };
    private readonly Entry _note = new() { Placeholder = "Optional problem note (included in export)", MaxLength = 500 };
    private bool _busy;

    internal DiagnosticPanel()
    {
        Content = new VerticalStackLayout
        {
            Spacing = 6, Children =
            {
                new Label { Text = "DIAGNOSTIC TEST BUILD v30.06 - simulation only", FontAttributes = FontAttributes.Bold },
                new Label { Text = "Available: manifest/minute inspection, source comparison, clear and cancel. Full backtests and sunflower menus are not implemented. Recording collects app action names, UTC times, durations, managed-memory samples and device/OS/display details. No file names, file contents, passwords, typed contract labels or device identifiers are logged. Your optional problem notes are included." },
                _status, _start, _stop, _note, _mark, _export, _clear
            }
        };
        _start.Clicked += async (_, _) => await RunAsync(StartAsync);
        _stop.Clicked += async (_, _) => await RunAsync(StopAsync);
        _mark.Clicked += (_, _) =>
        {
            AppDiagnostics.Current?.Record(DiagnosticAction.ProblemMarked, userNote: _note.Text);
            _note.Text = string.Empty;
            _status.Text = "Problem marker recorded. Continue testing or export.";
        };
        _export.Clicked += async (_, _) => await RunAsync(ExportAsync);
        _clear.Clicked += async (_, _) => await RunAsync(ClearAsync);
        Refresh();
        var timer = Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromSeconds(10);
        timer.Tick += (_, _) =>
        {
            AppDiagnostics.Record(DiagnosticAction.MemorySample);
            if (AppDiagnostics.Current is { } journal && (journal.StorageFailed || journal.DroppedEvents > 0))
                _status.Text = journal.StorageFailed ? "Diagnostic storage unavailable. Testing may continue; export may be incomplete." : $"Recording limit/queue reached; {journal.DroppedEvents} events omitted. Stop and export.";
        };
        Loaded += (_, _) => timer.Start();
        Unloaded += (_, _) => timer.Stop();
    }

    private async Task RunAsync(Func<Task> operation)
    {
        if (_busy) return;
        _busy = true; Refresh();
        try { await operation(); }
        catch (Exception error) when (error is not OutOfMemoryException)
        { _status.Text = "Diagnostics action unavailable. No automatic upload occurred. Retry or clear diagnostics."; }
        finally { _busy = false; Refresh(); }
    }

    private async Task StartAsync()
    {
        if (AppDiagnostics.Current is not null) return;
        var display = DeviceDisplay.Current.MainDisplayInfo;
        var build = typeof(App).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
        var environment = new DiagnosticEnvironment(build, DeviceInfo.Current.Model, DeviceInfo.Current.Manufacturer,
            DeviceInfo.Current.VersionString, display.Width, display.Height, display.Density);
        var directory = AppDiagnostics.DirectoryPath;
        AppDiagnostics.Current = await Task.Run(() =>
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
            return new DiagnosticJournal(directory, environment);
        });
        _status.Text = "Recording ON - local only. Mark a problem if needed; Stop/Export when finished.";
    }

    private async Task StopAsync()
    {
        if (AppDiagnostics.Current is { } journal)
        {
            await journal.StopAsync();
            AppDiagnostics.Current = null;
            _status.Text = journal.StorageFailed ? "Recording stopped with a storage error; export may be incomplete." : "Recording stopped. Export your diagnostic ZIP.";
        }
    }

    private async Task ExportAsync()
    {
        AppDiagnostics.Record(DiagnosticAction.ExportRequested);
        await StopAsync();
        var root = Path.Combine(FileSystem.CacheDirectory, "sharing-root");
        Directory.CreateDirectory(root);
        var output = Path.Combine(root, "QuantForge-diagnostics.zip");
        if (File.Exists(output)) File.Delete(output);
        await Task.Run(() => DiagnosticJournal.ExportAsync(AppDiagnostics.DirectoryPath, output));
        await Share.Default.RequestAsync(new ShareFileRequest
        { Title = "Export QuantForge diagnostics", File = new ShareFile(output, "application/zip") });
        _status.Text = "ZIP prepared. Choose where to save/share, then attach it in this chat. Opening the share sheet does not confirm upload.";
    }

    private async Task ClearAsync()
    {
        await StopAsync();
        var directory = AppDiagnostics.DirectoryPath;
        await Task.Run(() => { if (Directory.Exists(directory)) Directory.Delete(directory, true); });
        var export = Path.Combine(FileSystem.CacheDirectory, "sharing-root", "QuantForge-diagnostics.zip");
        if (File.Exists(export)) File.Delete(export);
        _note.Text = string.Empty;
        _status.Text = "Diagnostics cleared on this device. Previously shared copies are not deleted.";
    }

    private void Refresh()
    {
        var recording = AppDiagnostics.Current is not null;
        _start.IsEnabled = !_busy && !recording;
        _stop.IsEnabled = _mark.IsEnabled = _note.IsEnabled = !_busy && recording;
        _export.IsEnabled = !_busy && (recording || File.Exists(Path.Combine(AppDiagnostics.DirectoryPath, "events.jsonl")));
        _clear.IsEnabled = !_busy;
        if (!recording && !_busy && _export.IsEnabled && _status.Text.StartsWith("Diagnostics OFF", StringComparison.Ordinal))
            _status.Text = "Diagnostics OFF. A previous session is available to export, including after an unexpected exit. Start replaces it.";
    }
}
