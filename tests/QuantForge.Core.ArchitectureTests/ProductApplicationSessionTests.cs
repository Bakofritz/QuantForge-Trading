namespace QuantForge.Core.ArchitectureTests;

public sealed class ProductApplicationSessionTests
{
    [Fact]
    public void Startup_HasNoResultsAndCannotExecute()
    {
        var session = new ProductApplicationSession();
        Assert.Equal(ProductSessionStatus.AwaitingData, session.Status);
        Assert.Equal("QF-AWAITING-DATA", session.DiagnosticCode);
        Assert.Null(session.State);
        Assert.False(session.ResearchCommandsEnabled);
        Assert.Throws<InvalidOperationException>(() => session.ValidateCommand(Start()));
    }

    [Theory]
    [InlineData("null")]
    [InlineData("identity")]
    [InlineData("mode")]
    [InlineData("section")]
    [InlineData("duplicate")]
    [InlineData("report-state")]
    public void RejectedRefresh_ClearsOldStateAndAllowsExplicitValidReload(string failure)
    {
        var session = new ProductApplicationSession();
        var summary = Summary();
        session.Load(summary, ProductWorkspaceSection.Research);
        Assert.Equal(ProductSessionStatus.Ready, session.Status);
        Assert.NotNull(session.State);
        Assert.True(session.ResearchCommandsEnabled);

        ResearchWorkflowSummary? rejected = failure switch
        {
            "null" => null,
            "identity" => summary with { WorkflowFingerprint = "" },
            "mode" => summary with { Mode = (ResearchBatchMode)999 },
            "duplicate" => summary with { TotalRuns = 2, InvalidRuns = 2,
                Reports = new[] { summary.Reports[0], summary.Reports[0] } },
            "report-state" => summary with { Reports = new[] { summary.Reports[0] with { Status = (ResearchResultStatus)999 } } },
            _ => summary
        };
        session.Load(rejected, failure == "section" ? (ProductWorkspaceSection)999 : ProductWorkspaceSection.Research);
        Assert.Equal(ProductSessionStatus.Invalid, session.Status);
        Assert.Equal("QF-PRESENTATION-REJECTED", session.DiagnosticCode);
        Assert.Null(session.State);
        Assert.False(session.ResearchCommandsEnabled);
        Assert.Throws<InvalidOperationException>(() => session.ValidateCommand(Start()));

        session.Load(summary, ProductWorkspaceSection.Research);
        Assert.Equal(ProductSessionStatus.Ready, session.Status);
        Assert.True(session.ResearchCommandsEnabled);
        var decision = session.ValidateCommand(Start());
        Assert.False(decision.CanSubmitOrders);
        Assert.False(decision.CanChangeApplicationSettings);
    }

    [Fact]
    public void DataBlocked_StillAllowsViewingButCannotExecute()
    {
        var session = new ProductApplicationSession();
        session.Load(Summary() with { Reliability = Array.Empty<DataReliabilityAssessment>() }, ProductWorkspaceSection.Research);
        Assert.Equal(ProductSessionStatus.DataBlocked, session.Status);
        Assert.NotNull(session.State);
        Assert.False(session.ResearchCommandsEnabled);
        Assert.Throws<InvalidOperationException>(() => session.ValidateCommand(Start()));
        var decision = session.ValidateCommand(new ProductUiCommand(ProductUiOperation.ViewResearchSummary, AuthorityDomain.ReadOnlyResearch));
        Assert.False(decision.CanSubmitOrders);
    }

    [Fact]
    public void UnexpectedFailure_PropagatesWithoutRetainingOldState()
    {
        var session = new ProductApplicationSession();
        session.Load(Summary(), ProductWorkspaceSection.Research);
        Assert.Throws<NotSupportedException>(() => session.Load(
            Summary() with { Reports = new UnavailableReports() }, ProductWorkspaceSection.Research));
        Assert.Null(session.State);
        Assert.False(session.ResearchCommandsEnabled);
        Assert.Equal(ProductSessionStatus.Invalid, session.Status);
        Assert.Equal("QF-PRESENTATION-REJECTED", session.DiagnosticCode);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    [InlineData(2147483647)]
    public void UnknownAuthority_IsRejectedAtCoreAndSessionBoundaries(int rawDomain)
    {
        var domain = (AuthorityDomain)rawDomain;
        Assert.Throws<InvalidOperationException>(() => AuthorityBoundary.EvaluateResearch(domain));
        var session = new ProductApplicationSession();
        session.Load(Summary(), ProductWorkspaceSection.Research);
        Assert.Throws<InvalidOperationException>(() => session.ValidateCommand(Start() with { Authority = domain }));
    }

    private static ProductUiCommand Start() =>
        new(ProductUiOperation.StartResearch, AuthorityDomain.ReadOnlyResearch);

    private static ResearchWorkflowSummary Summary() =>
        new("workflow-a", ResearchBatchMode.ReadOnlyResearch, 1, 0, 0, 1, 100m, 100m,
            new[] { new ResearchReport("job-a", ResearchResultStatus.Invalid, "dataset-a", "strategy-a",
                "execution-a", "parameters-a", "partition-a", "Invalid research parameters.", null, null) },
            new[] { new DataReliabilityAssessment("dataset-a", 100m, true, 0, 0, null) });

    private sealed class UnavailableReports : IReadOnlyList<ResearchReport>
    {
        public int Count => throw new NotSupportedException("Sensitive internal failure detail.");
        public ResearchReport this[int index] => throw new NotSupportedException();
        public IEnumerator<ResearchReport> GetEnumerator() => throw new NotSupportedException();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
