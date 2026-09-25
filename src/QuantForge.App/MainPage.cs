using Microsoft.Maui;
using Microsoft.Maui.Controls;
using QuantForge.Core;
using Microsoft.Maui.Storage;

namespace QuantForge.App;

public sealed class MainPage : ContentPage
{
    private readonly Button _inspectManifestButton = new() { Text = "Inspect research manifest" };
    private readonly Label _manifestLabel = new() { Text = "No manifest inspected. Inspection does not admit data or enable research." };
    private readonly Entry _instrumentEntry = new() { Placeholder = "Declared contract, e.g. MES 09-26", MaxLength = 64 };
    private readonly Picker _priceSeriesPicker = new() { Title = "Declared price series", ItemsSource = new[] { "Last", "Bid", "Ask" } };
    private readonly Button _inspectDataButton = new() { Text = "Inspect NT8 one-minute export (UTC)" };
    private readonly Label _dataLabel = new() { Text = "No market data inspected. Contract and price series must be declared; text rows cannot verify them." };
    private readonly Button _cancelInspectionButton = new() { Text = "Cancel inspection", IsEnabled = false };
    private CancellationTokenSource? _inspectionCancellation;
    private readonly ProductApplicationSession _session = new();
    private readonly Label _diagnosticLabel = new();
    private readonly Label _workflowLabel = new();
    private readonly Label _sectionLabel = new();
    private readonly Label _reliabilityLabel = new();
    private readonly Label _researchCommandLabel = new();
    private readonly Label _liveAuthorityLabel = new();
    private readonly VerticalStackLayout _jobsLayout = new() { Spacing = 8 };

    public MainPage()
    {
        Title = "QuantForge";
        _inspectManifestButton.Clicked += async (_, _) => await InspectManifestAsync();

        _inspectDataButton.Clicked += async (_, _) => await InspectDataAsync();
        _cancelInspectionButton.Clicked += (_, _) => CancelInspection();

        ShowUnavailable("Awaiting validated research data", _session.DiagnosticCode);

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 24,
                Spacing = 12,
                Children =
                {
                    new Label
                    {
                        Text = "QuantForge",
                        FontSize = 30,
                        FontAttributes = FontAttributes.Bold
                    },
                    new Label
                    {
                        Text = "Research workspace shell"
                    },
                    _inspectManifestButton,
                    _manifestLabel,
                    _instrumentEntry,
                    _priceSeriesPicker,
                    _inspectDataButton,
                    _dataLabel,
                    _cancelInspectionButton,
                    _diagnosticLabel,
                    _workflowLabel,
                    _sectionLabel,
                    _reliabilityLabel,
                    _researchCommandLabel,
                    _liveAuthorityLabel,
                    new Label
                    {
                        Text = "Research jobs",
                        FontAttributes = FontAttributes.Bold
                    },
                    _jobsLayout,
                    new Label
                    {
                        Text = "This shell displays validated read-only research state and does not hold broker, order-submission, or application-setting authority."
                    }
                }
            }
        };
    }

    private async Task InspectManifestAsync()
    {
        if (_inspectionCancellation is not null) return;
        using var cancellation = BeginInspection();
        _manifestLabel.Text = "Manifest inspection pending. No data admitted.";
        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Choose a QuantForge research manifest" });
            if (file is null)
            {
                _manifestLabel.Text = "Manifest selection cancelled. No data admitted.";
                return;
            }
            cancellation.Token.ThrowIfCancellationRequested();
            using var stream = await file.OpenReadAsync();
            cancellation.Token.ThrowIfCancellationRequested();
            cancellation.CancelAfter(TimeSpan.FromSeconds(30));
            var result = await ResearchManifestReader.InspectAsync(stream, cancellation.Token);
            cancellation.Token.ThrowIfCancellationRequested();
            _manifestLabel.Text = result.Manifest is { } manifest
                ? $"Manifest inspected | Dataset: {manifest.DatasetId} | Strategy: {manifest.StrategyId} | Source SHA-256: {result.SourceFingerprint}. Data and strategy remain unadmitted."
                : $"Manifest inspection: {result.Status} | {result.DiagnosticCode}. No data admitted; choose a valid file to retry.";
        }
        catch (OperationCanceledException)
        {
            _manifestLabel.Text = "Manifest selection cancelled. No data admitted.";
        }
        catch (Exception error) when (error is not OutOfMemoryException)
        {
            // Native picker/provider failures are contained at this UI boundary.
            // Never display raw exception details or treat a failed read as admission.
            _manifestLabel.Text = "Manifest file could not be inspected | QF-MANIFEST-UNAVAILABLE. No data admitted; retry selection.";
        }
        finally
        {
            EndInspection();
        }
    }

    private async Task InspectDataAsync()
    {
        if (_inspectionCancellation is not null) return;
        _dataLabel.Text = "Market-data inspection pending. Research remains disabled.";
        if (string.IsNullOrWhiteSpace(_instrumentEntry.Text) || _priceSeriesPicker.SelectedIndex < 0)
        {
            _dataLabel.Text = "Declare the contract and Last/Bid/Ask series before choosing a UTC one-minute NT8 export.";
            return;
        }
        var descriptor = new Nt8MinuteDescriptor(_instrumentEntry.Text, (MarketPriceSeries)_priceSeriesPicker.SelectedIndex);
        using var cancellation = BeginInspection();
        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Choose UTC NT8 one-minute text export (up to 8 MiB)" });
            if (file is null)
            {
                _dataLabel.Text = "Market-data selection cancelled. No data admitted.";
                return;
            }
            cancellation.Token.ThrowIfCancellationRequested();
            using var stream = await file.OpenReadAsync();
            cancellation.Token.ThrowIfCancellationRequested();
            cancellation.CancelAfter(TimeSpan.FromSeconds(30));
            var result = await Task.Run(() => Nt8MinuteInspector.InspectAsync(stream, descriptor, cancellation.Token));
            cancellation.Token.ThrowIfCancellationRequested();
            _dataLabel.Text = result.Bars is { Count: > 0 } bars
                ? $"Inspected {bars.Count} bars | Declared: {descriptor.Instrument}, {descriptor.PriceSeries} | UTC end stamps {bars[0].Timestamp:u} to {bars[^1].Timestamp:u} | Non-contiguous intervals: {result.NonContiguousIntervals} (not classified as missing data) | SHA-256: {result.SourceFingerprint}. Identity, session coverage and benchmark remain unverified; research disabled."
                : $"Inspection: {result.Status} | {result.DiagnosticCode} | line {result.ErrorLine}. No data admitted; retry with a supported export.";
        }
        catch (OperationCanceledException) { _dataLabel.Text = "Market-data selection cancelled. No data admitted."; }
        catch (Exception error) when (error is not OutOfMemoryException)
        { _dataLabel.Text = "Market-data provider unavailable. No data admitted; retry selection."; }
        finally
        {
            EndInspection();
        }
    }

    private CancellationTokenSource BeginInspection()
    {
        _inspectionCancellation = new CancellationTokenSource();
        _inspectDataButton.IsEnabled = _inspectManifestButton.IsEnabled = false;
        _instrumentEntry.IsEnabled = _priceSeriesPicker.IsEnabled = false;
        _cancelInspectionButton.IsEnabled = true;
        return _inspectionCancellation;
    }

    private void EndInspection()
    {
        _inspectionCancellation = null;
        _inspectDataButton.IsEnabled = _inspectManifestButton.IsEnabled = true;
        _instrumentEntry.IsEnabled = _priceSeriesPicker.IsEnabled = true;
        _cancelInspectionButton.IsEnabled = false;
    }

    private void CancelInspection()
    {
        if (_inspectionCancellation is null) return;
        _inspectionCancellation.Cancel();
        _cancelInspectionButton.IsEnabled = false;
        _manifestLabel.Text = "Inspection cancellation requested. Waiting for the file provider to return; no data admitted.";
        _dataLabel.Text = "Inspection cancellation requested. Waiting for the file provider to return; no data admitted.";
    }

    protected override void OnDisappearing()
    {
        CancelInspection();
        base.OnDisappearing();
    }

    public void ApplySummary(
        ResearchWorkflowSummary? summary,
        ProductWorkspaceSection section)
    {
        ShowUnavailable("Validating research data", "QF-PRESENTATION-LOADING");
        try
        {
            _session.Load(summary, section);
        }
        finally
        {
            if (_session.State is null)
                ShowUnavailable("Research data could not be displayed. Correct the input and retry.", _session.DiagnosticCode);
        }
        if (_session.State is not { } state)
            return;

        Bind(state);
        _diagnosticLabel.Text = $"Session: {_session.Status} | {_session.DiagnosticCode}";
    }

    private void ShowUnavailable(string message, string diagnostic)
    {
        _diagnosticLabel.Text = $"{message} | {diagnostic}";
        _workflowLabel.Text = "Workflow: not loaded";
        _sectionLabel.Text = "Workspace: awaiting validated state";
        _reliabilityLabel.Text = "Data reliability: not admitted";
        _researchCommandLabel.Text = "Research commands: disabled";
        _liveAuthorityLabel.Text = "Live trading: disabled";
        _jobsLayout.Children.Clear();
    }

    private void Bind(ProductApplicationViewModel state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (state.LiveAccountEnabled || state.CanSubmitOrders || state.CanChangeApplicationSettings)
            throw new InvalidOperationException("The QuantForge application shell cannot bind authority-escalated product state.");

        if (string.IsNullOrWhiteSpace(state.WorkflowFingerprint))
            throw new InvalidOperationException("The QuantForge application shell requires workflow identity.");

        _workflowLabel.Text = $"Workflow: {state.WorkflowFingerprint}";
        _sectionLabel.Text = $"Workspace: {state.ActiveSection}";
        _reliabilityLabel.Text = state.HasBlockingDataIssues
            ? "Data reliability: BLOCKED — research execution is disabled until the reported data issue is resolved."
            : $"Data reliability: READY — minimum {FormatScore(state.MinimumReliabilityScore)}, average {FormatScore(state.AverageReliabilityScore)}";
        _researchCommandLabel.Text = state.ResearchCommandsEnabled
            ? "Research commands: enabled"
            : "Research commands: disabled";
        _liveAuthorityLabel.Text = "Live trading: disabled";

        _jobsLayout.Children.Clear();
        foreach (var job in state.Jobs)
        {
            var details = job.State switch
            {
                ProductUiJobState.Complete =>
                    $"{job.JobFingerprint}: Complete | evidence {job.EvidenceFingerprint}",
                ProductUiJobState.DataBlocked =>
                    $"{job.JobFingerprint}: Data blocked | {job.Message}",
                ProductUiJobState.Invalid =>
                    $"{job.JobFingerprint}: Invalid | {job.Message}",
                ProductUiJobState.Pending =>
                    $"{job.JobFingerprint}: Pending",
                ProductUiJobState.Running =>
                    $"{job.JobFingerprint}: Running",
                _ => throw new InvalidOperationException("Unknown product application job state.")
            };

            _jobsLayout.Children.Add(new Label { Text = details });
        }
    }

    private static string FormatScore(decimal? score) =>
        score is null ? "n/a" : $"{score.Value:0.##}%";
}
