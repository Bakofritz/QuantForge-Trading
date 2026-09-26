namespace QuantForge.Core.ArchitectureTests;

public sealed class ProductApplicationCoordinatorTests
{
    [Fact]
    public void Present_WiresValidatedWorkflowSummaryIntoApplicationViewModel()
    {
        var summary = new ResearchWorkflowSummary(
            "workflow-safe",
            ResearchBatchMode.ReadOnlyResearch,
            TotalRuns: 1,
            CompleteRuns: 0,
            DataBlockedRuns: 0,
            InvalidRuns: 1,
            MinimumReliabilityScore: 100m,
            AverageReliabilityScore: 100m,
            new[]
            {
                new ResearchReport(
                    "job-invalid",
                    ResearchResultStatus.Invalid,
                    "dataset-a",
                    "strategy-a",
                    "execution-a",
                    "parameters-a",
                    "partition-a",
                    "Invalid test state.",
                    null,
                    null)
            },
            new[]
            {
                new DataReliabilityAssessment("dataset-a", 100m, true, 0, 0, null)
            });

        var coordinator = new ProductApplicationCoordinator();
        var state = coordinator.Present(summary, ProductWorkspaceSection.Research);

        Assert.Equal("workflow-safe", state.WorkflowFingerprint);
        Assert.Equal(ProductWorkspaceSection.Research, state.ActiveSection);
        Assert.True(state.ResearchCommandsEnabled);
        Assert.False(state.LiveAccountEnabled);
        Assert.False(state.CanSubmitOrders);
        Assert.False(state.CanChangeApplicationSettings);
    }

    [Fact]
    public void ValidateCommand_RejectsOperationOutsideActiveSection()
    {
        var coordinator = new ProductApplicationCoordinator();
        var state = CreateApplicationState(
            ProductWorkspaceSection.Reports,
            hasBlockingDataIssue: false);

        var error = Assert.Throws<InvalidOperationException>(() =>
            coordinator.ValidateCommand(
                state,
                new ProductUiCommand(ProductUiOperation.StartResearch, AuthorityDomain.ReadOnlyResearch)));

        Assert.Contains("not available", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateCommand_BlocksResearchExecutionWhenReliabilityBlocksAdmission()
    {
        var coordinator = new ProductApplicationCoordinator();
        var state = CreateApplicationState(
            ProductWorkspaceSection.Research,
            hasBlockingDataIssue: true);

        var error = Assert.Throws<InvalidOperationException>(() =>
            coordinator.ValidateCommand(
                state,
                new ProductUiCommand(ProductUiOperation.StartResearch, AuthorityDomain.ReadOnlyResearch)));

        Assert.Contains("data reliability", error.Message, StringComparison.OrdinalIgnoreCase);

        var viewDecision = coordinator.ValidateCommand(
            state,
            new ProductUiCommand(ProductUiOperation.ViewResearchSummary, AuthorityDomain.ReadOnlyResearch));

        Assert.False(viewDecision.CanSubmitOrders);
        Assert.False(viewDecision.CanChangeApplicationSettings);
    }

    [Fact]
    public void ValidateCommand_RejectsLiveAuthorityEvenForAllowedOperation()
    {
        var coordinator = new ProductApplicationCoordinator();
        var state = CreateApplicationState(
            ProductWorkspaceSection.Reports,
            hasBlockingDataIssue: false);

        var error = Assert.Throws<InvalidOperationException>(() =>
            coordinator.ValidateCommand(
                state,
                new ProductUiCommand(ProductUiOperation.ViewResearchSummary, AuthorityDomain.LiveAccount)));

        Assert.Contains("Live-account authority", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static ProductApplicationViewModel CreateApplicationState(
        ProductWorkspaceSection section,
        bool hasBlockingDataIssue)
    {
        var reliability = hasBlockingDataIssue
            ? new[] { new DataReliabilityAssessment("dataset-a", 95m, true, 1, 0, "Gap blocks admission.") }
            : new[] { new DataReliabilityAssessment("dataset-a", 100m, true, 0, 0, null) };

        var workflow = new ProductUiWorkflowState(
            "workflow-a",
            ResearchBatchMode.ReadOnlyResearch,
            TotalRuns: 1,
            CompleteRuns: 0,
            DataBlockedRuns: 0,
            InvalidRuns: 1,
            MinimumReliabilityScore: reliability[0].ScorePercent,
            AverageReliabilityScore: reliability[0].ScorePercent,
            LiveAccountEnabled: false,
            CanSubmitOrders: false,
            CanChangeApplicationSettings: false,
            new[]
            {
                new ProductUiRunState(
                    "job-invalid",
                    ResearchResultStatus.Invalid,
                    "dataset-a",
                    "strategy-a",
                    "Invalid test state.",
                    null)
            },
            reliability);

        return ProductApplicationViewModelRules.Create(workflow, section);
    }
}

public sealed class ProductApplicationCoordinatorWorkflowResultTests
{
    [Fact]
    public void Workflow_result_presentation_uses_read_only_ui_boundary()
    {
        var report = new ResearchReport("job", ResearchResultStatus.DataBlocked, "dataset", "strategy", "timing", "params", "wf", "blocked", null, null);
        var result = new ResearchWorkflowResult(
            new[] { report },
            new[] { new ResearchComponentStatus("job", ResearchComponentState.DataBlocked, "blocked") },
            new[] { new DataReliabilityAssessment("dataset", false, 1, 0, "blocked") });

        var view = new ProductApplicationCoordinator().PresentWorkflowResult(result, ResearchBatchMode.ReadOnlyResearch, ProductWorkspaceSection.Reports);
        Assert.False(view.LiveAccountEnabled);
        Assert.False(view.CanSubmitOrders);
        Assert.False(view.CanChangeApplicationSettings);
        Assert.Equal(ProductUiJobState.DataBlocked, view.Jobs[0].State);
    }
}
