using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public class Phase3ExitContractTests
{
    [Fact]
    public void Phase3_research_workflow_preserves_reliability_causality_isolation_export_and_publishable_provenance()
    {
        var first = Request("params-a", "job-a", "batch|s1|account-a");
        var second = Request("params-b", "job-b", "batch|s1|account-b");
        var plan = ResearchOptimizationPlanRules.Create(new[] { first, second });
        var reliability = new DataReliabilityAssessment("data-sha", 99m, true, 0, 0, null);
        var decision = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var contexts = new[]
        {
            Context("job-a", decision),
            Context("job-b", decision)
        };

        var result = ResearchWorkflowCoordinator.Run(new ResearchWorkflowRequest(
            new ResearchBatchRequest(ResearchBatchMode.ReadOnlyOptimization, plan.Variants),
            new[] { reliability },
            contexts));

        Assert.Equal(2, result.Reports.Count);
        Assert.All(result.Reports, report => Assert.Equal(ResearchResultStatus.Complete, report.Status));
        Assert.All(result.ComponentStatuses, status => Assert.Equal(ResearchComponentState.Complete, status.State));
        Assert.NotEqual(result.Reports[0].EvidenceTail!.Value.Fingerprint, result.Reports[1].EvidenceTail!.Value.Fingerprint);
        Assert.NotEqual(result.Reports[0].Account!.Value.LedgerNamespace, result.Reports[1].Account!.Value.LedgerNamespace);

        var summary = ResearchWorkflowSummaryFactory.Create(ResearchBatchMode.ReadOnlyOptimization, result);
        var markdown = ResearchWorkflowMarkdownExporter.Export(summary);

        Assert.Equal(2, summary.CompleteRuns);
        Assert.Equal(99m, summary.MinimumReliabilityScore);
        Assert.False(string.IsNullOrWhiteSpace(summary.WorkflowFingerprint));
        Assert.Contains(summary.WorkflowFingerprint, markdown, StringComparison.Ordinal);
        Assert.Contains("99%", markdown, StringComparison.Ordinal);

        var source = new ProvenanceRecord(
            "artifact-1",
            "local://strategy-1",
            "2026-01-01T00:00:00Z",
            "source-sha",
            "original-sha",
            "strategy-1",
            "scrub-1");

        var firstPublication = ResearchPublicationRules.Bind(first.Job, source, result.Reports[0], reliability);
        var secondPublication = ResearchPublicationRules.Bind(second.Job, source, result.Reports[1], reliability);

        Assert.Equal("job-a", firstPublication.Provenance.JobFingerprint);
        Assert.Equal("job-b", secondPublication.Provenance.JobFingerprint);
        Assert.Equal("data-sha", firstPublication.Reliability.DatasetFingerprint);
        Assert.Equal("data-sha", secondPublication.Reliability.DatasetFingerprint);
    }

    [Fact]
    public void Phase3_workflow_fails_closed_for_future_higher_timeframe_information()
    {
        var run = Request("params-a", "job-a", "batch|s1|account-a");
        var decision = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var futureContext = new ResearchTimeframeContext(
            "job-a",
            decision,
            new[]
            {
                new TimeframeObservation(
                    "5m",
                    decision.AddMinutes(-1),
                    decision.AddMinutes(4),
                    decision.AddMinutes(4))
            });
        var reliability = new DataReliabilityAssessment("data-sha", 99m, true, 0, 0, null);

        var result = ResearchWorkflowCoordinator.Run(new ResearchWorkflowRequest(
            new ResearchBatchRequest(ResearchBatchMode.ReadOnlyResearch, new[] { run }),
            new[] { reliability },
            new[] { futureContext }));

        var report = Assert.Single(result.Reports);
        Assert.Equal(ResearchResultStatus.Invalid, report.Status);
        Assert.Null(report.Account);
        Assert.Null(report.EvidenceTail);
    }

    [Fact]
    public void Phase3_workflow_fails_closed_for_unresolved_data_and_unsafe_strategy_authority()
    {
        var run = Request("params-a", "job-a", "batch|s1|account-a");
        var unreliable = new DataReliabilityAssessment(
            "data-sha",
            80m,
            true,
            1,
            0,
            "One source interval remains unresolved and was not inferred.");

        var blocked = ResearchWorkflowCoordinator.Run(new ResearchWorkflowRequest(
            new ResearchBatchRequest(ResearchBatchMode.ReadOnlyResearch, new[] { run }),
            new[] { unreliable }));

        var blockedReport = Assert.Single(blocked.Reports);
        Assert.Equal(ResearchResultStatus.DataBlocked, blockedReport.Status);
        Assert.Null(blockedReport.Account);
        Assert.Null(blockedReport.EvidenceTail);

        var unsafeRun = run with
        {
            Job = run.Job with
            {
                Strategy = run.Job.Strategy with { CanSubmitOrders = true }
            }
        };

        Assert.Throws<InvalidOperationException>(() =>
            ResearchOptimizationPlanRules.Create(new[] { unsafeRun }));
    }

    private static ResearchTimeframeContext Context(string jobFingerprint, DateTimeOffset decision) =>
        new(
            jobFingerprint,
            decision,
            new[]
            {
                new TimeframeObservation(
                    "1m",
                    decision.AddMinutes(-2),
                    decision.AddMinutes(-1),
                    decision.AddMinutes(-1)),
                new TimeframeObservation(
                    "5m",
                    decision.AddMinutes(-10),
                    decision.AddMinutes(-5),
                    decision.AddMinutes(-5))
            });

    private static ResearchRunRequest Request(
        string parameterFingerprint,
        string jobFingerprint,
        string ledgerNamespace)
    {
        var signal = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var data = new DataAdmission(
            "dataset",
            "data-sha",
            "TEST",
            "1m",
            signal.AddHours(-1),
            signal.AddHours(1),
            true,
            true);
        var strategy = new StrategyCapabilityManifest(
            "s1",
            "strategy-1",
            false,
            false,
            false,
            false,
            false,
            false,
            false);
        var identity = new ResearchJobIdentity(
            "data-sha",
            "strategy-1",
            "exec-sha",
            parameterFingerprint,
            "partition",
            jobFingerprint);
        var job = new ResearchJobSpec(
            AuthorityDomain.SimulatedAccount,
            data,
            strategy,
            identity,
            ExecutionTimingPolicy.NextBarOpen,
            false);
        var intent = new SimulationIntent(
            "s1",
            ledgerNamespace,
            SimulationSide.Buy,
            SimulationIntentType.Market,
            1m,
            signal,
            signal.AddMinutes(1));
        var market = new MarketEvent(
            1,
            signal.AddMinutes(1),
            100m,
            100m,
            100m,
            100m,
            10m);
        var admission = new StrategyAdmissionEnvelope(
            StrategyAdmissionState.Admitted,
            strategy,
            new StrategyFeatureSelection(new[] { StrategyFeature.SignalGeneration }),
            "quarantine-strategy-1",
            "strategy-1");

        return new ResearchRunRequest(
            job,
            new[] { intent },
            new[] { market },
            1000m,
            0m,
            0m,
            admission);
    }
}
