namespace QuantForge.Core.ArchitectureTests;

public sealed class Phase5ReliabilityTests
{
    [Theory]
    [InlineData("missing")]
    [InlineData("unrelated")]
    [InlineData("duplicate")]
    public void IncompleteOrAmbiguousReliability_BlocksExecutionButPreservesReports(string scenario)
    {
        var good = new DataReliabilityAssessment("dataset-a", 97m, true, 0, 0, null);
        var assessments = scenario switch
        {
            "missing" => Array.Empty<DataReliabilityAssessment>(),
            "unrelated" => new[] { good with { DatasetFingerprint = "other" } },
            _ => new[] { good, good }
        };
        var state = ProductApplicationViewModelRules.Create(Workflow(assessments), ProductWorkspaceSection.Research);
        Assert.True(state.HasBlockingDataIssues);
        Assert.False(state.ResearchCommandsEnabled);
        Assert.Null(state.MinimumReliabilityScore);
        Assert.Null(state.AverageReliabilityScore);
        Assert.Single(state.Jobs);
        var coordinator = new ProductApplicationCoordinator();
        Assert.Throws<InvalidOperationException>(() => coordinator.ValidateCommand(state,
            new ProductUiCommand(ProductUiOperation.StartResearch, AuthorityDomain.HistoricalResearch)));
        coordinator.ValidateCommand(state,
            new ProductUiCommand(ProductUiOperation.ViewResearchSummary, AuthorityDomain.HistoricalResearch));
    }

    [Fact]
    public void Scores_AreDerivedFromMatchingAssessmentsInsteadOfUntrustedSummaryNumbers()
    {
        var state = ProductApplicationViewModelRules.Create(
            Workflow(new[] { new DataReliabilityAssessment("dataset-a", 97m, true, 0, 0, null) }),
            ProductWorkspaceSection.Research);
        Assert.True(state.ResearchCommandsEnabled);
        Assert.Equal(97m, state.MinimumReliabilityScore);
        Assert.Equal(97m, state.AverageReliabilityScore);
        Assert.False(state.LiveAccountEnabled);
    }

    private static ProductUiWorkflowState Workflow(IReadOnlyList<DataReliabilityAssessment> reliability) =>
        new("workflow", ResearchBatchMode.ReadOnlyResearch, 1, 0, 0, 1, 100m, 100m,
            false, false, false,
            new[] { new ProductUiRunState("job", ResearchResultStatus.Invalid,
                "dataset-a", "strategy-a", "Invalid parameters.", null) }, reliability);
}
