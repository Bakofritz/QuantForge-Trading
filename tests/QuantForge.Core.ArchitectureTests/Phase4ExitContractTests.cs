namespace QuantForge.Core.ArchitectureTests;

public sealed class Phase4ExitContractTests
{
    [Fact]
    public void ResearchSummary_FlowsIntoApplicationWithoutTradingAuthority()
    {
        var coordinator = new ProductApplicationCoordinator();
        var state = coordinator.Present(
            CreateResearchSummary(blockingReliability: false),
            ProductWorkspaceSection.Research);

        Assert.Equal("phase4-workflow", state.WorkflowFingerprint);
        Assert.Equal(ProductWorkspaceSection.Research, state.ActiveSection);
        Assert.True(state.ResearchCommandsEnabled);
        Assert.False(state.LiveAccountEnabled);
        Assert.False(state.CanSubmitOrders);
        Assert.False(state.CanChangeApplicationSettings);
        Assert.Equal(2, state.Jobs.Count);
        Assert.Contains(ProductUiOperation.StartResearch, state.AllowedOperations);
        Assert.DoesNotContain(ProductUiOperation.StartOptimization, state.AllowedOperations);
    }

    [Fact]
    public void ReliabilityBlock_DisablesExecutionButPreservesReadOnlySummaryAccess()
    {
        var coordinator = new ProductApplicationCoordinator();
        var state = coordinator.Present(
            CreateResearchSummary(blockingReliability: true),
            ProductWorkspaceSection.Research);

        Assert.True(state.HasBlockingDataIssues);
        Assert.False(state.ResearchCommandsEnabled);

        var executeError = Assert.Throws<InvalidOperationException>(() =>
            coordinator.ValidateCommand(
                state,
                new ProductUiCommand(ProductUiOperation.StartResearch, AuthorityDomain.ReadOnlyResearch)));
        Assert.Contains("data reliability", executeError.Message, StringComparison.OrdinalIgnoreCase);

        var viewDecision = coordinator.ValidateCommand(
            state,
            new ProductUiCommand(ProductUiOperation.ViewResearchSummary, AuthorityDomain.ReadOnlyResearch));

        Assert.False(viewDecision.CanSubmitOrders);
        Assert.False(viewDecision.CanChangeApplicationSettings);
    }

    [Fact]
    public void BlockedAndInvalidJobs_RemainExplicitRetryableNonPerformanceStates()
    {
        var state = new ProductApplicationCoordinator().Present(
            CreateResearchSummary(blockingReliability: true),
            ProductWorkspaceSection.Reports);

        var blocked = Assert.Single(state.Jobs.Where(x => x.State == ProductUiJobState.DataBlocked));
        Assert.True(blocked.CanRetry);
        Assert.Null(blocked.EvidenceFingerprint);
        Assert.False(string.IsNullOrWhiteSpace(blocked.Message));

        var invalid = Assert.Single(state.Jobs.Where(x => x.State == ProductUiJobState.Invalid));
        Assert.True(invalid.CanRetry);
        Assert.Null(invalid.EvidenceFingerprint);
        Assert.False(string.IsNullOrWhiteSpace(invalid.Message));
    }

    [Fact]
    public void OptimizationWorkspace_RemainsReadOnlyAndLedgerSafeAtUiBoundary()
    {
        var summary = CreateResearchSummary(blockingReliability: false) with
        {
            Mode = ResearchBatchMode.ReadOnlyOptimization
        };

        var state = new ProductApplicationCoordinator().Present(
            summary,
            ProductWorkspaceSection.Optimization);

        Assert.Contains(ProductUiOperation.StartOptimization, state.AllowedOperations);
        Assert.DoesNotContain(ProductUiOperation.StartResearch, state.AllowedOperations);
        Assert.False(state.LiveAccountEnabled);
        Assert.False(state.CanSubmitOrders);
        Assert.False(state.CanChangeApplicationSettings);
        Assert.Equal(2, state.Jobs.Select(x => x.JobFingerprint).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void ForgedCompleteUiJobWithoutEvidence_FailsClosed()
    {
        var workflow = new ProductUiWorkflowState(
            "forged-workflow",
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
                    "job-forged",
                    ResearchResultStatus.Complete,
                    "dataset-safe",
                    "strategy-safe",
                    null,
                    null)
            },
            new[]
            {
                new DataReliabilityAssessment("dataset-safe", 100m, true, 0, 0, null)
            });

        var error = Assert.Throws<InvalidOperationException>(() =>
            ProductApplicationViewModelRules.Create(
                workflow,
                ProductWorkspaceSection.Reports));

        Assert.Contains("requires execution evidence", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LiveAccountCommand_IsRejectedAtFinalProductCoordinatorBoundary()
    {
        var coordinator = new ProductApplicationCoordinator();
        var state = coordinator.Present(
            CreateResearchSummary(blockingReliability: false),
            ProductWorkspaceSection.Reports);

        var error = Assert.Throws<InvalidOperationException>(() =>
            coordinator.ValidateCommand(
                state,
                new ProductUiCommand(ProductUiOperation.ViewResearchSummary, AuthorityDomain.LiveAccount)));

        Assert.Contains("Live-account authority", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static ResearchWorkflowSummary CreateResearchSummary(bool blockingReliability)
    {
        var reliability = blockingReliability
            ? new[]
            {
                new DataReliabilityAssessment(
                    "dataset-blocked",
                    92m,
                    true,
                    1,
                    0,
                    "Unresolved gap blocks research admission.")
            }
            : new[]
            {
                new DataReliabilityAssessment(
                    "dataset-blocked",
                    100m,
                    true,
                    0,
                    0,
                    null)
            };

        return new ResearchWorkflowSummary(
            "phase4-workflow",
            ResearchBatchMode.ReadOnlyResearch,
            TotalRuns: 2,
            CompleteRuns: 0,
            DataBlockedRuns: 1,
            InvalidRuns: 1,
            MinimumReliabilityScore: reliability[0].ScorePercent,
            AverageReliabilityScore: reliability[0].ScorePercent,
            new[]
            {
                new ResearchReport(
                    "job-blocked",
                    ResearchResultStatus.DataBlocked,
                    "dataset-blocked",
                    "strategy-a",
                    "execution-a",
                    "parameters-a",
                    "partition-a",
                    "Research data is blocked for this run.",
                    null,
                    null),
                new ResearchReport(
                    "job-invalid",
                    ResearchResultStatus.Invalid,
                    "dataset-blocked",
                    "strategy-b",
                    "execution-b",
                    "parameters-b",
                    "partition-b",
                    "Strategy admission is invalid for this run.",
                    null,
                    null)
            },
            reliability);
    }
}
