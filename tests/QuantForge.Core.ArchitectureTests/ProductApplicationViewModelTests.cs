namespace QuantForge.Core.ArchitectureTests;

public sealed class ProductApplicationViewModelTests
{
    [Fact]
    public void Create_MapsWorkflowIntoReadOnlyApplicationState()
    {
        var workflow = new ProductUiWorkflowState(
            "workflow-a",
            ResearchBatchMode.ReadOnlyResearch,
            TotalRuns: 2,
            CompleteRuns: 1,
            DataBlockedRuns: 1,
            InvalidRuns: 0,
            MinimumReliabilityScore: 90m,
            AverageReliabilityScore: 95m,
            LiveAccountEnabled: false,
            CanSubmitOrders: false,
            CanChangeApplicationSettings: false,
            new[]
            {
                new ProductUiRunState(
                    "job-complete",
                    ResearchResultStatus.Complete,
                    "dataset-a",
                    "strategy-a",
                    null,
                    "evidence-a"),
                new ProductUiRunState(
                    "job-blocked",
                    ResearchResultStatus.DataBlocked,
                    "dataset-b",
                    "strategy-b",
                    "Unresolved market-data gap.",
                    null)
            },
            new[]
            {
                new DataReliabilityAssessment("dataset-a", 100m, true, 0, 0, null),
                new DataReliabilityAssessment("dataset-b", 90m, true, 1, 0, "One unresolved gap blocks admission.")
            });

        var state = ProductApplicationViewModelRules.Create(
            workflow,
            ProductWorkspaceSection.Research);

        Assert.Equal("workflow-a", state.WorkflowFingerprint);
        Assert.True(state.HasBlockingDataIssues);
        Assert.False(state.ResearchCommandsEnabled);
        Assert.False(state.LiveAccountEnabled);
        Assert.False(state.CanSubmitOrders);
        Assert.False(state.CanChangeApplicationSettings);
        Assert.Equal(2, state.Jobs.Count);
        Assert.Contains(state.Jobs, x => x.State == ProductUiJobState.Complete && !x.CanRetry);
        Assert.Contains(state.Jobs, x => x.State == ProductUiJobState.DataBlocked && x.CanRetry);
        Assert.Contains(ProductUiOperation.StartResearch, state.AllowedOperations);
        Assert.DoesNotContain(ProductUiOperation.StartOptimization, state.AllowedOperations);
    }

    [Fact]
    public void Create_RejectsAuthorityEscalationFromForgedUiState()
    {
        var workflow = new ProductUiWorkflowState(
            "workflow-live",
            ResearchBatchMode.ReadOnlyResearch,
            TotalRuns: 1,
            CompleteRuns: 0,
            DataBlockedRuns: 0,
            InvalidRuns: 1,
            MinimumReliabilityScore: null,
            AverageReliabilityScore: null,
            LiveAccountEnabled: true,
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
            Array.Empty<DataReliabilityAssessment>());

        var error = Assert.Throws<InvalidOperationException>(() =>
            ProductApplicationViewModelRules.Create(workflow, ProductWorkspaceSection.Research));

        Assert.Contains("cannot accept live", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_RejectsCompletedJobWithoutExecutionEvidence()
    {
        var workflow = new ProductUiWorkflowState(
            "workflow-missing-evidence",
            ResearchBatchMode.ReadOnlyResearch,
            TotalRuns: 1,
            CompleteRuns: 1,
            DataBlockedRuns: 0,
            InvalidRuns: 0,
            MinimumReliabilityScore: 100m,
            AverageReliabilityScore: 100m,
            LiveAccountEnabled: false,
            CanSubmitOrders: false,
            CanChangeApplicationSettings: false,
            new[]
            {
                new ProductUiRunState(
                    "job-complete",
                    ResearchResultStatus.Complete,
                    "dataset-a",
                    "strategy-a",
                    null,
                    null)
            },
            new[]
            {
                new DataReliabilityAssessment("dataset-a", 100m, true, 0, 0, null)
            });

        var error = Assert.Throws<InvalidOperationException>(() =>
            ProductApplicationViewModelRules.Create(workflow, ProductWorkspaceSection.Reports));

        Assert.Contains("requires execution evidence", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void OptimizationSection_ExposesOptimizationOperationWithoutTradingAuthority()
    {
        var workflow = new ProductUiWorkflowState(
            "workflow-opt",
            ResearchBatchMode.ReadOnlyOptimization,
            TotalRuns: 1,
            CompleteRuns: 0,
            DataBlockedRuns: 0,
            InvalidRuns: 1,
            MinimumReliabilityScore: 100m,
            AverageReliabilityScore: 100m,
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
                    "Invalid parameter variant.",
                    null)
            },
            new[]
            {
                new DataReliabilityAssessment("dataset-a", 100m, true, 0, 0, null)
            });

        var state = ProductApplicationViewModelRules.Create(
            workflow,
            ProductWorkspaceSection.Optimization);

        Assert.True(state.ResearchCommandsEnabled);
        Assert.Contains(ProductUiOperation.StartOptimization, state.AllowedOperations);
        Assert.DoesNotContain(ProductUiOperation.StartResearch, state.AllowedOperations);
        Assert.False(state.CanSubmitOrders);
        Assert.False(state.CanChangeApplicationSettings);
    }
}
