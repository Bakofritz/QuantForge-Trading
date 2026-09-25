namespace QuantForge.Core.ArchitectureTests;

public sealed class ProductUiBoundaryTests
{
    [Fact]
    public void LiveAccountCommand_IsRejectedByProductUiBoundary()
    {
        var command = new ProductUiCommand(
            ProductUiOperation.StartResearch,
            AuthorityDomain.LiveAccount);

        var error = Assert.Throws<InvalidOperationException>(() =>
            ProductUiBoundary.RequireResearchOnly(command));

        Assert.Contains("Live-account authority", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReadOnlyState_PreservesResearchStatusAndCannotAcquireTradingAuthority()
    {
        var reports = new[]
        {
            new ResearchReport(
                "job-blocked",
                ResearchResultStatus.DataBlocked,
                "dataset-a",
                "strategy-a",
                "execution-a",
                "parameters-a",
                "partition-a",
                "Unresolved market-data gap.",
                null,
                null),
            new ResearchReport(
                "job-invalid",
                ResearchResultStatus.Invalid,
                "dataset-b",
                "strategy-b",
                "execution-b",
                "parameters-b",
                "partition-b",
                "Strategy admission failed.",
                null,
                null)
        };

        var reliability = new[]
        {
            new DataReliabilityAssessment(
                "dataset-a",
                91m,
                true,
                1,
                0,
                "One unresolved gap blocks research admission."),
            new DataReliabilityAssessment(
                "dataset-b",
                100m,
                true,
                0,
                0,
                null)
        };

        var summary = new ResearchWorkflowSummary(
            "workflow-fingerprint",
            ResearchBatchMode.ReadOnlyResearch,
            TotalRuns: 2,
            CompleteRuns: 0,
            DataBlockedRuns: 1,
            InvalidRuns: 1,
            MinimumReliabilityScore: 91m,
            AverageReliabilityScore: 95.5m,
            reports,
            reliability);

        var state = ProductUiBoundary.CreateReadOnlyState(summary);

        Assert.Equal("workflow-fingerprint", state.WorkflowFingerprint);
        Assert.Equal(2, state.TotalRuns);
        Assert.False(state.LiveAccountEnabled);
        Assert.False(state.CanSubmitOrders);
        Assert.False(state.CanChangeApplicationSettings);
        Assert.Equal(2, state.Runs.Count);
        Assert.Contains(state.Runs, x => x.Status == ResearchResultStatus.DataBlocked && x.Message is not null);
        Assert.Contains(state.Runs, x => x.Status == ResearchResultStatus.Invalid && x.Message is not null);
        Assert.Equal(2, state.Reliability.Count);
    }

    [Fact]
    public void ReadOnlyState_RejectsInconsistentWorkflowCounts()
    {
        var report = new ResearchReport(
            "job-invalid",
            ResearchResultStatus.Invalid,
            "dataset-a",
            "strategy-a",
            "execution-a",
            "parameters-a",
            "partition-a",
            "Invalid test job.",
            null,
            null);

        var summary = new ResearchWorkflowSummary(
            "workflow-fingerprint",
            ResearchBatchMode.ReadOnlyResearch,
            TotalRuns: 1,
            CompleteRuns: 1,
            DataBlockedRuns: 1,
            InvalidRuns: 0,
            MinimumReliabilityScore: null,
            AverageReliabilityScore: null,
            new[] { report },
            Array.Empty<DataReliabilityAssessment>());

        Assert.Throws<InvalidOperationException>(() =>
            ProductUiBoundary.CreateReadOnlyState(summary));
    }
}
