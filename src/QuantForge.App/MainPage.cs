using Microsoft.Maui;
using Microsoft.Maui.Controls;
using QuantForge.Core;
using Microsoft.Maui.Storage;

namespace QuantForge.App;

public sealed class MainPage : ContentPage
{
    private readonly Button _inspectManifestButton = new() { Text = "Inspect research manifest" };
    private readonly Label _manifestLabel = new() { Text = "No manifest inspected. Inspection does not admit data or enable research." };
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
        _inspectManifestButton.IsEnabled = false;
        _manifestLabel.Text = "Manifest inspection pending. No data admitted.";
        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Choose a QuantForge research manifest" });
            if (file is null)
            {
                _manifestLabel.Text = "Manifest selection cancelled. No data admitted.";
                return;
            }
            using var stream = await file.OpenReadAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var result = await ResearchManifestReader.InspectAsync(stream, timeout.Token);
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
            _inspectManifestButton.IsEnabled = true;
        }
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
