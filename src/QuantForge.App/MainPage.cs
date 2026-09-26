using System.Diagnostics;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using QuantForge.Core;
using Microsoft.Maui.Storage;

namespace QuantForge.App;

public sealed class MainPage : ContentPage
{
    private readonly Button _batchButton = new() { Text = "Choose multiple market-data TXT / ZIP files" };
    private readonly Button _clearBatchButton = new() { Text = "Clear batch", IsEnabled = false };
    private readonly Label _batchLabel = new() { Text = "Mix MES/MNQ and minute/day/tick files. Up to 64 files/members, 32 MiB per TXT, 64 MiB total ZIP input, 128 MiB total expanded text and 1,000,000 retained rows. Unclear identities stay unresolved; no research admission." };
    private readonly Microsoft.Maui.Controls.Switch _utcDayComparison = new();
    private MarketBatchResult? _batch;
    private readonly Button _inspectManifestButton = new() { Text = "Inspect QuantForge JSON manifest" };
    private readonly Label _manifestLabel = new() { Text = "Advanced: QuantForge research metadata (.json), up to 64 KiB. Market-data TXT files do not belong here." };
    private readonly Label _fileDetailsLabel = new() { Text = "Contract and Last/Bid/Ask will be read from the filename. Labels remain unverified." };
    private readonly Button _inspectDataButton = new() { Text = "Choose market-data TXT (UTC one-minute)" };
    private readonly Label _dataLabel = new() { Text = "Choose an original NT8 UTC one-minute export. Up to 32 MiB / 500,000 bars. Use the batch control above for daily and tick files." };
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
        _batchButton.Clicked += async (_, _) => await InspectBatchAsync();
        _clearBatchButton.Clicked += (_, _) => { _batch = null; _batchLabel.Text = "Batch cleared. Original files unchanged."; _clearBatchButton.IsEnabled = false; AppDiagnostics.Record(DiagnosticAction.DataCleared); };

        _inspectManifestButton.Clicked += async (_, _) => await InspectManifestAsync();

        _inspectDataButton.Clicked += async (_, _) => await InspectDataAsync();
        _cancelInspectionButton.Clicked += (_, _) => CancelInspection();
        _compareDataButton.Clicked += async (_, _) => await CompareDataAsync();
        _clearDataButton.Clicked += (_, _) => { AppDiagnostics.Record(DiagnosticAction.DataCleared); ClearInspectedData(); };
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
                    new DiagnosticPanel(),
                    new Label { Text = "Market data", FontAttributes = FontAttributes.Bold },
                    _batchButton,
                    new Label { Text = "Optional: compare daily bars using UTC calendar days (exploratory, not exchange sessions). Applies to next batch." },
                    _utcDayComparison,
                    _clearBatchButton,
                    _batchLabel,
                    new Label { Text = "Single-file minute inspection and manual reference comparison" },
                    _inspectDataButton,
                    _fileDetailsLabel,
                    _dataLabel,
                    _compareDataButton,
                    _clearDataButton,
                    _comparisonLabel,
                    _cancelInspectionButton,
                    new Label { Text = "Advanced: research metadata", FontAttributes = FontAttributes.Bold },
                    _manifestLabel,
                    _inspectManifestButton,
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

    private async Task InspectBatchAsync()
    {
        if (_inspectionCancellation is not null) return;
        _batch = null;
        ClearInspectedData();
        var utcDays = _utcDayComparison.IsToggled;
        using var cancellation = BeginInspection();
        var clock = Stopwatch.StartNew();
        AppDiagnostics.Record(DiagnosticAction.BatchInspection, DiagnosticOutcome.Started);
        _batchLabel.Text = "Choose TXT files and/or ZIP archives. No data admitted.";
        try
        {
            var picked = await FilePicker.Default.PickMultipleAsync(new PickOptions { PickerTitle = "Select market-data TXT and ZIP files" });
            cancellation.Token.ThrowIfCancellationRequested();
            var files = picked?.Take(MarketBatchInspection.MaximumFiles + 1).ToArray();
            if (files is null || files.Length == 0)
            {
                AppDiagnostics.Record(DiagnosticAction.BatchInspection, DiagnosticOutcome.Cancelled, clock.ElapsedMilliseconds);
                _batchLabel.Text = "Batch selection cancelled."; return;
            }
            var processing = Stopwatch.StartNew();
            cancellation.CancelAfter(TimeSpan.FromMinutes(2));
            var sources = files.Select(f => new MarketBatchSource(f.FileName, () => f.OpenReadAsync())).ToArray();
            var result = await Task.Run(() => MarketBatchInspection.InspectAsync(sources, cancellation.Token));
            cancellation.Token.ThrowIfCancellationRequested();
            var cross = await Task.Run(() => MarketCrossValidation.Compare(result, utcDays, cancellation.Token));
            cancellation.Token.ThrowIfCancellationRequested();
            _batch = result.Completed ? result : null;
            AppDiagnostics.Record(DiagnosticAction.BatchProcessing, DiagnosticOutcome.Observed, processing.ElapsedMilliseconds);
            AppDiagnostics.Record(DiagnosticAction.BatchInspection, result.Completed ? DiagnosticOutcome.Completed : DiagnosticOutcome.Invalid, clock.ElapsedMilliseconds);
            AppDiagnostics.Record(DiagnosticAction.CrossValidation, result.Completed ? DiagnosticOutcome.Completed : DiagnosticOutcome.Invalid);
            var lines = new List<string> { $"{result.Code} | {result.Files.Count} file results. Inspection only; research/live disabled." };
            foreach (var group in result.Files.GroupBy(f => f.Label?.Instrument ?? "Unresolved instrument").OrderBy(g => g.Key))
            {
                lines.Add($"GROUP: {group.Key} (unverified)");
                foreach (var f in group)
                    lines.Add($"#{f.Index} {f.Name}: {f.State} | {f.Data?.Kind} | {f.Label?.PriceSeries} | {f.Data?.Rows?.Count ?? 0} rows | {f.Code} | {f.LabelBasis}" +
                        (f.Data?.Hash is { } h ? $" | SHA-256 {h}" : "") + (f.DuplicateOf is { } dupe ? $" | duplicate of #{dupe}" : ""));
            }
            lines.Add("CROSS-VALIDATION (unverified labels; observed coverage only)");
            foreach (var pair in cross.Pairs)
                lines.Add($"#{pair.Left} vs #{pair.Right}: {pair.Code} | {pair.Matching} matching, {pair.Conflicting} differing, {pair.LeftOnly}/{pair.RightOnly} only-left/right | {pair.Basis}");
            lines.Add($"{cross.OmittedPairs} eligible pairs omitted by limit. {cross.Limitation}");
            _batchLabel.Text = string.Join("\n\n", lines);
        }
        catch (OperationCanceledException)
        {
            _batch = null; _batchLabel.Text = "Batch cancelled/timed out. No batch results retained.";
            AppDiagnostics.Record(DiagnosticAction.BatchInspection, DiagnosticOutcome.Cancelled, clock.ElapsedMilliseconds);
        }
        catch (Exception error) when (error is not OutOfMemoryException)
        {
            _batch = null; _batchLabel.Text = "Batch provider/processing unavailable. No batch results retained.";
            AppDiagnostics.Record(DiagnosticAction.BatchInspection, DiagnosticOutcome.Unavailable, clock.ElapsedMilliseconds);
        }
        finally { EndInspection(); }
    }

    private async Task InspectManifestAsync()
    {
        if (_inspectionCancellation is not null) return;
        using var cancellation = BeginInspection();
        var actionClock = Stopwatch.StartNew();
        AppDiagnostics.Record(DiagnosticAction.ManifestInspection, DiagnosticOutcome.Started);
        _manifestLabel.Text = "Manifest inspection pending. No data admitted.";
        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Choose QuantForge metadata (.json), not market-data TXT" });
            if (file is null)
            {
                AppDiagnostics.Record(DiagnosticAction.ManifestInspection, DiagnosticOutcome.Cancelled, actionClock.ElapsedMilliseconds);
                _manifestLabel.Text = "Manifest selection cancelled. No data admitted.";
                return;
            }
            if (!file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                AppDiagnostics.Record(DiagnosticAction.InspectionReason, DiagnosticOutcome.WrongFileType);
                AppDiagnostics.Record(DiagnosticAction.ManifestInspection, DiagnosticOutcome.Invalid, actionClock.ElapsedMilliseconds);
                _manifestLabel.Text = "This control accepts QuantForge JSON metadata only. For minute TXT files, use Choose market-data TXT above. Use batch inspection above for daily and tick data.";
                return;
            }
            cancellation.Token.ThrowIfCancellationRequested();
            using var stream = await file.OpenReadAsync();
            cancellation.Token.ThrowIfCancellationRequested();
            cancellation.CancelAfter(TimeSpan.FromSeconds(30));
            var result = await ResearchManifestReader.InspectAsync(stream, cancellation.Token);
            cancellation.Token.ThrowIfCancellationRequested();
            AppDiagnostics.Record(DiagnosticAction.ManifestInspection, result.Status switch { ManifestInspectionStatus.Inspected => DiagnosticOutcome.Completed, ManifestInspectionStatus.Unavailable => DiagnosticOutcome.Unavailable, ManifestInspectionStatus.Cancelled => DiagnosticOutcome.Cancelled, _ => DiagnosticOutcome.Invalid }, actionClock.ElapsedMilliseconds);
            AppDiagnostics.Record(DiagnosticAction.InspectionReason, ImportInspectionFeedback.Classify(result.DiagnosticCode));
            _manifestLabel.Text = result.Manifest is { } manifest
                ? $"Manifest inspected | Dataset: {manifest.DatasetId} | Strategy: {manifest.StrategyId} | Source SHA-256: {result.SourceFingerprint}. Data and strategy remain unadmitted."
                : $"{ImportInspectionFeedback.Describe(result.DiagnosticCode)} | {result.DiagnosticCode}. No data admitted.";
        }
        catch (OperationCanceledException)
        {
            AppDiagnostics.Record(DiagnosticAction.ManifestInspection, DiagnosticOutcome.Cancelled, actionClock.ElapsedMilliseconds);
            _manifestLabel.Text = "Manifest selection cancelled. No data admitted.";
        }
        catch (Exception error) when (error is not OutOfMemoryException)
        {
            AppDiagnostics.Record(DiagnosticAction.ManifestInspection, DiagnosticOutcome.Unavailable, actionClock.ElapsedMilliseconds);
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
        using var cancellation = BeginInspection();
        var actionClock = Stopwatch.StartNew();
        AppDiagnostics.Record(DiagnosticAction.DataInspection, DiagnosticOutcome.Started);
        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Choose UTC NT8 one-minute text export (up to 32 MiB)" });
            if (file is null)
            {
                AppDiagnostics.Record(DiagnosticAction.DataInspection, DiagnosticOutcome.Cancelled, actionClock.ElapsedMilliseconds);
                _dataLabel.Text = "Market-data selection cancelled. No data admitted.";
                return;
            }
            cancellation.Token.ThrowIfCancellationRequested();
            var descriptor = Nt8FileLabelRules.Detect(file.FileName);
            AppDiagnostics.Record(DiagnosticAction.FileLabelDetected, descriptor is null ? DiagnosticOutcome.Invalid : DiagnosticOutcome.Completed);
            if (descriptor is null)
            {
                _dataLabel.Text = "Unrecognized NT8 filename. Expected a contract and series, for example MES 09-26.Last.txt. Do not rename unrelated data to force acceptance.";
                AppDiagnostics.Record(DiagnosticAction.InspectionReason, DiagnosticOutcome.InvalidFileLabel);
                AppDiagnostics.Record(DiagnosticAction.DataInspection, DiagnosticOutcome.Invalid, actionClock.ElapsedMilliseconds);
                return;
            }
            _fileDetailsLabel.Text = $"Filename labels: {descriptor.Instrument}, {descriptor.PriceSeries}. Contents identity, interval and timezone are not proven by the filename.";
            var processingClock = Stopwatch.StartNew();
            using var stream = await file.OpenReadAsync();
            cancellation.Token.ThrowIfCancellationRequested();
            cancellation.CancelAfter(TimeSpan.FromSeconds(30));
            var result = await Task.Run(() => Nt8MinuteInspector.InspectAsync(stream, descriptor, cancellation.Token));
            cancellation.Token.ThrowIfCancellationRequested();
            AppDiagnostics.Record(DiagnosticAction.DataProcessing, DiagnosticOutcome.Observed, processingClock.ElapsedMilliseconds);
            AppDiagnostics.Record(DiagnosticAction.DataInspection, result.Status switch { MarketDataInspectionStatus.Inspected => DiagnosticOutcome.Completed, MarketDataInspectionStatus.Unavailable => DiagnosticOutcome.Unavailable, MarketDataInspectionStatus.Cancelled => DiagnosticOutcome.Cancelled, _ => DiagnosticOutcome.Invalid }, actionClock.ElapsedMilliseconds);
            AppDiagnostics.Record(DiagnosticAction.InspectionReason, ImportInspectionFeedback.Classify(result.DiagnosticCode));
            _inspectedData = result.Status == MarketDataInspectionStatus.Inspected ? result : null;
            _dataLabel.Text = result.Bars is { Count: > 0 } bars
                ? $"Inspected {bars.Count} bars | Declared: {descriptor.Instrument}, {descriptor.PriceSeries} | UTC end stamps {bars[0].Timestamp:u} to {bars[^1].Timestamp:u} | Non-contiguous intervals: {result.NonContiguousIntervals} (not classified as missing data) | SHA-256: {result.SourceFingerprint}. Filename matches the declaration only. Contents identity, session coverage and benchmark remain unverified; research disabled."
                : $"{ImportInspectionFeedback.Describe(result.DiagnosticCode)} | {result.DiagnosticCode} | line {result.ErrorLine}. No data admitted.";
        }
        catch (OperationCanceledException) { AppDiagnostics.Record(DiagnosticAction.DataInspection, DiagnosticOutcome.Cancelled, actionClock.ElapsedMilliseconds); _dataLabel.Text = "Market-data selection cancelled. No data admitted."; }
        catch (Exception error) when (error is not OutOfMemoryException)
        { AppDiagnostics.Record(DiagnosticAction.DataInspection, DiagnosticOutcome.Unavailable, actionClock.ElapsedMilliseconds); _dataLabel.Text = "Market-data provider unavailable. No data admitted; retry selection."; }
        finally
        {
            EndInspection();
        }
    }

    private void ClearInspectedData()
    {
        _inspectedData = null;
        _fileDetailsLabel.Text = "No file selected. Contract and Last/Bid/Ask will be read from the filename; labels remain unverified.";
        _compareDataButton.IsEnabled = _clearDataButton.IsEnabled = false;
        _dataLabel.Text = "No market data retained. Contract and series declarations do not verify file identity.";
        _comparisonLabel.Text = "No source comparison. Agreement is not live-benchmark verification.";
    }

    private async Task CompareDataAsync()
    {
        if (_inspectionCancellation is not null || _inspectedData is not { DeclaredDescriptor: { } descriptor } primary) return;
        _comparisonLabel.Text = "Comparing declared sources. No reliability or research admission granted.";
        using var cancellation = BeginInspection();
        var actionClock = Stopwatch.StartNew();
        AppDiagnostics.Record(DiagnosticAction.SourceComparison, DiagnosticOutcome.Started);
        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions
            { PickerTitle = $"Choose another UTC minute export declared as {descriptor.Instrument} {descriptor.PriceSeries}" });
            if (file is null)
            {
                AppDiagnostics.Record(DiagnosticAction.SourceComparison, DiagnosticOutcome.Cancelled, actionClock.ElapsedMilliseconds);
                _comparisonLabel.Text = "Reference selection cancelled. No comparison evidence created.";
                return;
            }
            cancellation.Token.ThrowIfCancellationRequested();
            var fileLabel = Nt8FileLabelRules.Check(file.FileName, descriptor);
            AppDiagnostics.Record(DiagnosticAction.FileLabelCheck, fileLabel == Nt8FileLabelStatus.MatchingDeclaredLabel ? DiagnosticOutcome.Completed : DiagnosticOutcome.Invalid);
            if (fileLabel != Nt8FileLabelStatus.MatchingDeclaredLabel)
            {
                _comparisonLabel.Text = $"File rejected: {fileLabel}. Choose an original NT8 file named CONTRACT.Last/Bid/Ask.txt that matches the declaration (for example MES 09-26.Last.txt). No data admitted. A matching filename alone does not verify contents.";
                AppDiagnostics.Record(DiagnosticAction.SourceComparison, DiagnosticOutcome.Invalid, actionClock.ElapsedMilliseconds);
                return;
            }
            var processingClock = Stopwatch.StartNew();
            using var stream = await file.OpenReadAsync();
            cancellation.Token.ThrowIfCancellationRequested();
            cancellation.CancelAfter(TimeSpan.FromSeconds(30));
            var reference = await Task.Run(() => Nt8MinuteInspector.InspectAsync(stream, descriptor, cancellation.Token));
            AppDiagnostics.Record(DiagnosticAction.InspectionReason, ImportInspectionFeedback.Classify(reference.DiagnosticCode));
            var result = await Task.Run(() => MinuteSeriesComparison.Compare(primary, reference, cancellation.Token));
            cancellation.Token.ThrowIfCancellationRequested();
            AppDiagnostics.Record(DiagnosticAction.ComparisonProcessing, DiagnosticOutcome.Observed, processingClock.ElapsedMilliseconds);
            AppDiagnostics.Record(DiagnosticAction.SourceComparison, result.Status switch { MinuteComparisonStatus.Compared => DiagnosticOutcome.Completed, MinuteComparisonStatus.Cancelled => DiagnosticOutcome.Cancelled, _ => DiagnosticOutcome.Invalid }, actionClock.ElapsedMilliseconds);
            _comparisonLabel.Text = result.Status == MinuteComparisonStatus.Compared
                ? $"Source agreement: {result.MatchingBars} matching, {result.ConflictingBars} differing, {result.PrimaryOnlyBars} only in primary, {result.ReferenceOnlyBars} only in reference. Same source bytes: {result.SameSourceBytes}. Primary SHA-256: {result.PrimaryFingerprint} | Reference SHA-256: {result.ReferenceFingerprint}. File labels match the declaration only. Full observed ranges compared; neither source identity, session coverage nor independence is verified. Research remains disabled."
                : $"Comparison unavailable: {result.DiagnosticCode}; reference inspection: {reference.DiagnosticCode}. Correct the reference and retry. Research remains disabled.";
        }
        catch (OperationCanceledException) { AppDiagnostics.Record(DiagnosticAction.SourceComparison, DiagnosticOutcome.Cancelled, actionClock.ElapsedMilliseconds); _comparisonLabel.Text = "Comparison cancelled. No comparison evidence created."; }
        catch (Exception error) when (error is not OutOfMemoryException)
        { AppDiagnostics.Record(DiagnosticAction.SourceComparison, DiagnosticOutcome.Unavailable, actionClock.ElapsedMilliseconds); _comparisonLabel.Text = "Reference provider unavailable. No comparison evidence created; retry selection."; }
        finally { EndInspection(); }
    }

    private CancellationTokenSource BeginInspection()
    {
        _inspectionCancellation = new CancellationTokenSource();
        _batchButton.IsEnabled = _utcDayComparison.IsEnabled = _clearBatchButton.IsEnabled = false;
        _inspectDataButton.IsEnabled = _inspectManifestButton.IsEnabled = false;
        _cancelInspectionButton.IsEnabled = true;
        _compareDataButton.IsEnabled = _clearDataButton.IsEnabled = false;
        return _inspectionCancellation;
    }

    private void EndInspection()
    {
        _inspectionCancellation = null;
        _batchButton.IsEnabled = _utcDayComparison.IsEnabled = true;
        _clearBatchButton.IsEnabled = _batch is not null;
        _inspectDataButton.IsEnabled = _inspectManifestButton.IsEnabled = true;
        _cancelInspectionButton.IsEnabled = false;
        _compareDataButton.IsEnabled = _clearDataButton.IsEnabled = _inspectedData is not null;
    }

    private void CancelInspection()
    {
        if (_inspectionCancellation is null) return;
        AppDiagnostics.Record(DiagnosticAction.CancelRequested);
        _inspectionCancellation.Cancel();
        ClearInspectedData();
        _cancelInspectionButton.IsEnabled = false;
        _manifestLabel.Text = "Inspection cancellation requested. Waiting for the file provider to return; no data admitted.";
        _dataLabel.Text = "Inspection cancellation requested. Waiting for the file provider to return; no data admitted.";
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AppDiagnostics.Record(DiagnosticAction.PageAppeared);
    }

    protected override void OnDisappearing()
    {
        AppDiagnostics.Record(DiagnosticAction.PageDisappeared);
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
