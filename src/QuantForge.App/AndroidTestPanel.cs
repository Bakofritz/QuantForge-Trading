using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using QuantForge.Core;

namespace QuantForge.App;

internal sealed class AndroidTestPanel : ContentView
{
    private const string AcceptancePreferenceKey = "quantforge.android.acceptance.v1";
    private readonly Label _status = new();
    private readonly Button _copyIdentity = new() { Text = "Copy build identity" };
    private readonly Button _shareChecklist = new() { Text = "Share Android test checklist" };
    private readonly Button _shareProgress = new() { Text = "Share device test progress" };
    private readonly Button _resetProgress = new() { Text = "Reset device test progress" };
    private readonly Label _progress = new();
    private readonly AndroidAcceptanceTracker _tracker = new();

    internal AndroidTestPanel()
    {
        var identity = CurrentIdentity();
        var android = DeviceInfo.Current.Platform == DevicePlatform.Android;
        _status.Text = android
            ? $"Android test build {identity.Version} ({identity.Build}) on {DeviceInfo.Current.Manufacturer} {DeviceInfo.Current.Model}, Android {DeviceInfo.Current.VersionString}."
            : $"Android test center is available for packaging/testing. Current platform: {identity.Platform}.";
        RestoreProgress(identity);

        Content = new VerticalStackLayout
        {
            Spacing = 6,
            Children =
            {
                new Label { Text = "Android test center", FontAttributes = FontAttributes.Bold },
                _status,
                new Label { Text = "Use this panel to keep every device report tied to the exact build. The checklist does not grant data admission or trading authority." },
                _copyIdentity,
                _shareChecklist,
                _shareProgress,
                _resetProgress,
                _progress
            }
        };

        _progress.Text = _tracker.Render();

        _copyIdentity.Clicked += async (_, _) =>
        {
            var current = CurrentIdentity();
            await Clipboard.Default.SetTextAsync($"QuantForge {current.Version} build {current.Build} | {current.Platform} | {current.OperatingSystem}");
            _status.Text = "Exact build identity copied. Include it with manual device feedback when possible.";
        };
        _shareChecklist.Clicked += async (_, _) =>
        {
            var current = CurrentIdentity();
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = "QuantForge Android test checklist",
                Text = AndroidTestChecklist.Render(current)
            });
            _status.Text = "Android checklist prepared for sharing. Share-sheet display does not confirm upload.";
        };
        _shareProgress.Clicked += async (_, _) =>
        {
            var current = CurrentIdentity();
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = "QuantForge Android device test progress",
                Text = AndroidDeviceTestReport.Render(current, _tracker.States)
            });
            _status.Text = "Build-bound device test progress prepared for sharing. Share-sheet display does not confirm upload.";
        };
        _resetProgress.Clicked += (_, _) =>
        {
            _tracker.Reset();
            Preferences.Default.Remove(AcceptancePreferenceKey);
            _progress.Text = _tracker.Render();
            _status.Text = "Device test progress reset for this build. Diagnostics are unchanged.";
        };
    }

    internal void Record(AndroidTestMilestone milestone, bool passed)
    {
        _tracker.Record(milestone, passed);
        PersistProgress();
        _progress.Text = _tracker.Render();
    }

    private void RestoreProgress(AndroidBuildIdentity identity)
    {
        var persisted = Preferences.Default.Get(AcceptancePreferenceKey, string.Empty);
        if (AndroidAcceptanceSnapshot.TryParse(persisted, out var snapshot) && snapshot is not null && snapshot.Matches(identity))
        {
            _tracker.Restore(snapshot.States);
            _status.Text += " Build-bound device-test progress restored.";
            return;
        }
        if (!string.IsNullOrEmpty(persisted))
            Preferences.Default.Remove(AcceptancePreferenceKey);
    }

    private void PersistProgress()
    {
        var snapshot = AndroidAcceptanceSnapshot.Create(CurrentIdentity(), _tracker.States);
        Preferences.Default.Set(AcceptancePreferenceKey, snapshot.Serialize());
    }

    internal string? CurrentSnapshotText()
    {
        if (DeviceInfo.Current.Platform != DevicePlatform.Android) return null;
        return AndroidAcceptanceSnapshot.Create(CurrentIdentity(), _tracker.States).Serialize();
    }

    private static AndroidBuildIdentity CurrentIdentity() => new(
        AppInfo.Current.VersionString,
        AppInfo.Current.BuildString,
        DeviceInfo.Current.Platform.ToString(),
        DeviceInfo.Current.VersionString);
}
