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
    private readonly Editor _expected = new() { Placeholder = "Expected behavior (optional)", MaxLength = DiagnosticProblemNote.MaximumFieldLength, AutoSize = EditorAutoSizeOption.TextChanges };
    private readonly Editor _actual = new() { Placeholder = "Actual behavior (optional)", MaxLength = DiagnosticProblemNote.MaximumFieldLength, AutoSize = EditorAutoSizeOption.TextChanges };
    private readonly Func<string?>? _acceptanceSnapshotProvider;
    private bool _busy;

    internal DiagnosticPanel(Func<string?>? acceptanceSnapshotProvider = null)
    {
        _acceptanceSnapshotProvider = acceptanceSnapshotProvider;
        Content = new VerticalStackLayout
        {
            Spacing = 6, Children =
            {
                new Label { Text = $"DIAGNOSTIC TEST BUILD {AppInfo.Current.VersionString} - simulation only", FontAttributes = FontAttributes.Bold },
                new Label { Text = "Available: batch TXT/ZIP minute/day/tick inspection, source comparison, clear and cancel. Full backtests and sunflower menus are not implemented. Recording collects app action names, UTC times, durations, managed-memory samples and device/OS/display details. Android exports also include the build-bound test-progress snapshot. No file names, file contents, passwords, typed contract labels or device identifiers are logged. Your optional problem notes are included." },
                _status, _start, _stop, _expected, _actual, _mark, _export, _clear
            }
        };
        _start.Clicked += async (_, _) => await RunAsync(StartAsync);
        _stop.Clicked += async (_, _) => await RunAsync(StopAsync);
        _mark.Clicked += (_, _) =>
        {
            string note;
            try { note = DiagnosticProblemNote.Compose(_expected.Text, _actual.Text); }
            catch (ArgumentException)
            {
                _status.Text = "Enter expected behavior, actual behavior, or both before marking a problem.";
                return;
            }
            var queued = AppDiagnostics.Current?.Record(DiagnosticAction.ProblemMarked, userNote: note) == true;
            if (queued) { _expected.Text = string.Empty; _actual.Text = string.Empty; }
            _status.Text = queued ? "Expected/actual problem marker queued locally. Continue testing or export." : "Marker could not be queued; recording is stopped, full or unavailable.";
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
        var environment = new DiagnosticEnvironment(build, AppInfo.Current.VersionString, AppInfo.Current.BuildString,
            DeviceInfo.Current.Model, DeviceInfo.Current.Manufacturer, DeviceInfo.Current.VersionString,
            display.Width, display.Height, display.Density);
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
        var acceptanceSnapshot = _acceptanceSnapshotProvider?.Invoke();
        await Task.Run(() => DiagnosticJournal.ExportAsync(AppDiagnostics.DirectoryPath, output, acceptanceSnapshot));
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
        _expected.Text = string.Empty;
        _actual.Text = string.Empty;
        _status.Text = "Diagnostics cleared on this device. Previously shared copies are not deleted.";
    }

    private void Refresh()
    {
        var recording = AppDiagnostics.Current is not null;
        _start.IsEnabled = !_busy && !recording;
        _stop.IsEnabled = _mark.IsEnabled = _expected.IsEnabled = _actual.IsEnabled = !_busy && recording;
        _export.IsEnabled = !_busy && (recording || File.Exists(Path.Combine(AppDiagnostics.DirectoryPath, "events.jsonl")));
        _clear.IsEnabled = !_busy;
        if (!recording && !_busy && _export.IsEnabled && _status.Text.StartsWith("Diagnostics OFF", StringComparison.Ordinal))
            _status.Text = "Diagnostics OFF. A previous session is available to export, including after an unexpected exit. Start replaces it.";
    }
}
