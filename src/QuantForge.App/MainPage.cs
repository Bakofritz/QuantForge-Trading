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
    private Nt8MinuteInspectionResult? _inspectedData;
    private readonly Button _compareDataButton = new() { Text = "Compare export with same declared contract and series", IsEnabled = false };
    private readonly Button _clearDataButton = new() { Text = "Clear inspected market data", IsEnabled = false };
    private readonly Label _comparisonLabel = new() { Text = "No source comparison. Agreement is not live-benchmark verification." };
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
        _compareDataButton.Clicked += async (_, _) => await CompareDataAsync();
        _clearDataButton.Clicked += (_, _) => ClearInspectedData();
        _instrumentEntry.TextChanged += (_, _) => ClearInspectedData();
        _priceSeriesPicker.SelectedIndexChanged += (_, _) => ClearInspectedData();

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
                    _compareDataButton,
                    _clearDataButton,
                    _comparisonLabel,
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
        ClearInspectedData();
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
            _inspectedData = result.Status == MarketDataInspectionStatus.Inspected ? result : null;
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

    private void ClearInspectedData()
    {
        _inspectedData = null;
        _compareDataButton.IsEnabled = _clearDataButton.IsEnabled = false;
        _dataLabel.Text = "No market data retained. Contract and series declarations do not verify file identity.";
        _comparisonLabel.Text = "No source comparison. Agreement is not live-benchmark verification.";
    }

    private async Task CompareDataAsync()
    {
        if (_inspectionCancellation is not null || _inspectedData is not { DeclaredDescriptor: { } descriptor } primary) return;
        _comparisonLabel.Text = "Comparing declared sources. No reliability or research admission granted.";
        using var cancellation = BeginInspection();
        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions
            { PickerTitle = $"Choose another UTC minute export declared as {descriptor.Instrument} {descriptor.PriceSeries}" });
            if (file is null)
            {
                _comparisonLabel.Text = "Reference selection cancelled. No comparison evidence created.";
                return;
            }
            cancellation.Token.ThrowIfCancellationRequested();
            using var stream = await file.OpenReadAsync();
            cancellation.Token.ThrowIfCancellationRequested();
            cancellation.CancelAfter(TimeSpan.FromSeconds(30));
            var reference = await Task.Run(() => Nt8MinuteInspector.InspectAsync(stream, descriptor, cancellation.Token));
            var result = await Task.Run(() => MinuteSeriesComparison.Compare(primary, reference, cancellation.Token));
            cancellation.Token.ThrowIfCancellationRequested();
            _comparisonLabel.Text = result.Status == MinuteComparisonStatus.Compared
                ? $"Source agreement: {result.MatchingBars} matching, {result.ConflictingBars} differing, {result.PrimaryOnlyBars} only in primary, {result.ReferenceOnlyBars} only in reference. Same source bytes: {result.SameSourceBytes}. Primary SHA-256: {result.PrimaryFingerprint} | Reference SHA-256: {result.ReferenceFingerprint}. Full observed ranges compared; neither source identity, session coverage nor independence is verified. Research remains disabled."
                : $"Comparison unavailable: {result.DiagnosticCode}; reference inspection: {reference.DiagnosticCode}. Correct the reference and retry. Research remains disabled.";
        }
        catch (OperationCanceledException) { _comparisonLabel.Text = "Comparison cancelled. No comparison evidence created."; }
        catch (Exception error) when (error is not OutOfMemoryException)
        { _comparisonLabel.Text = "Reference provider unavailable. No comparison evidence created; retry selection."; }
        finally { EndInspection(); }
    }

    private CancellationTokenSource BeginInspection()
    {
        _inspectionCancellation = new CancellationTokenSource();
        _inspectDataButton.IsEnabled = _inspectManifestButton.IsEnabled = false;
        _instrumentEntry.IsEnabled = _priceSeriesPicker.IsEnabled = false;
        _cancelInspectionButton.IsEnabled = true;
        _compareDataButton.IsEnabled = _clearDataButton.IsEnabled = false;
        return _inspectionCancellation;
    }

    private void EndInspection()
    {
        _inspectionCancellation = null;
        _inspectDataButton.IsEnabled = _inspectManifestButton.IsEnabled = true;
        _instrumentEntry.IsEnabled = _priceSeriesPicker.IsEnabled = true;
        _cancelInspectionButton.IsEnabled = false;
        _compareDataButton.IsEnabled = _clearDataButton.IsEnabled = _inspectedData is not null;
    }

    private void CancelInspection()
    {
        if (_inspectionCancellation is null) return;
        _inspectionCancellation.Cancel();
        ClearInspectedData();
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
