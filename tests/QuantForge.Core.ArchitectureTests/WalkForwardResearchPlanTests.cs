using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class WalkForwardResearchPlanTests
{
    [Fact]
    public void Plan_rejects_future_market_data_in_training_window()
    {
        var segment = FixtureSegment();
        var future = segment.Optimization.Plan.Variants[0] with
        {
            MarketEvents = new[] { Bar(segment.EvaluationStart.AddMinutes(1), 1) }
        };
        var request = segment.Optimization with
        {
            Plan = new ResearchOptimizationPlan(new[] { future, segment.Optimization.Plan.Variants[1] })
        };

        Assert.Throws<InvalidOperationException>(() =>
            WalkForwardResearchPlanRules.Create(new[] { segment with { Optimization = request } }));
    }

    [Fact]
    public void Plan_requires_training_before_evaluation_and_distinct_partitions()
    {
        var segment = FixtureSegment();
        Assert.Throws<InvalidOperationException>(() => WalkForwardResearchPlanRules.Create(new[]
        {
            segment with { EvaluationStart = segment.TrainingEnd.AddMinutes(-1) }
        }));

        var samePartition = segment.EvaluationRun with
        {
            Job = segment.EvaluationRun.Job with
            {
                Identity = segment.EvaluationRun.Job.Identity with { TemporalPartitionId = "train-01" }
            }
        };
        Assert.Throws<InvalidOperationException>(() => WalkForwardResearchPlanRules.Create(new[]
        {
            segment with { EvaluationRun = samePartition }
        }));
    }

    internal static WalkForwardSegment FixtureSegment()
    {
        var trainStart = new DateTimeOffset(2026, 1, 5, 14, 30, 0, TimeSpan.Zero);
        var trainEnd = trainStart.AddMinutes(3);
        var evalStart = trainEnd;
        var evalEnd = evalStart.AddMinutes(3);
        var catalog = new DatasetCatalog();
        Register(catalog, "train", "train-data", trainStart, trainEnd);
        Register(catalog, "eval", "eval-data", evalStart, evalEnd);
        var manifest = new StrategyCapabilityManifest("s1", "strategy-sha", false, false, false, false, false, false, false);
        var admission = new StrategyAdmissionEnvelope(
            StrategyAdmissionState.Admitted, manifest,
            new StrategyFeatureSelection(new[] { StrategyFeature.SignalGeneration }),
            "quarantine-sha", "strategy-sha");
        var trainData = catalog.RequireAdmission("train");
        var evalData = catalog.RequireAdmission("eval");
        var a = Run(trainData, manifest, admission, "params-a", "train-01", "job-a", "wf|s1|a", trainStart.AddMinutes(1));
        var b = Run(trainData, manifest, admission, "params-b", "train-01", "job-b", "wf|s1|b", trainStart.AddMinutes(1));
        var evaluation = Run(evalData, manifest, admission, "params-a", "eval-01", "job-eval", "wf|s1|eval", evalStart.AddMinutes(1));
        var reliability = new[]
        {
            new DataReliabilityAssessment("train-data", 100m, true, 0, 0, null),
            new DataReliabilityAssessment("eval-data", 100m, true, 0, 0, null)
        };
        var coverage = new[]
        {
            new SessionCoverageReport("train-data", "session-train", 1, 1, 0, 0, Array.Empty<DateTimeOffset>(), Array.Empty<DateTimeOffset>(), true, "explicit"),
            new SessionCoverageReport("eval-data", "session-eval", 1, 1, 0, 0, Array.Empty<DateTimeOffset>(), Array.Empty<DateTimeOffset>(), true, "explicit")
        };
        var optimization = new ResearchOptimizationApplicationRequest(
            ResearchOptimizationPlanRules.Create(new[] { a, b }), catalog, new[] { admission }, reliability, coverage);
        return new WalkForwardSegment(
            "segment-01", trainStart, trainEnd, evalStart, evalEnd,
            optimization, ResearchOptimizationObjective.FinalEquity, evaluation);
    }

    private static ResearchRunRequest Run(
        DataAdmission data,
        StrategyCapabilityManifest manifest,
        StrategyAdmissionEnvelope admission,
        string parameters,
        string partition,
        string jobId,
        string ledger,
        DateTimeOffset time)
    {
        var identity = new ResearchJobIdentity(data.DatasetFingerprint, manifest.SourceFingerprint, "exec-sha", parameters, partition, jobId);
        var job = new ResearchJobSpec(AuthorityDomain.SimulatedAccount, data, manifest, identity, ExecutionTimingPolicy.NextBarOpen, false);
        var intent = new SimulationIntent("s1", ledger, SimulationSide.Buy, SimulationIntentType.Market, 1m, time.AddMinutes(-1), time);
        return new ResearchRunRequest(job, new[] { intent }, new[] { Bar(time, 1) }, 1000m, 0m, 0m, admission);
    }

    private static MarketEvent Bar(DateTimeOffset time, long sequence) =>
        new(sequence, time, 100m, 100m, 100m, 100m, 10m);

    private static void Register(DatasetCatalog catalog, string id, string fingerprint, DateTimeOffset start, DateTimeOffset end) =>
        catalog.Register(new DatasetCatalogEntry(
            id, fingerprint, "TEST", "1m", start, end, true,
            new ProvenanceRecord(id, "fixture://" + id, "2026-01-01T00:00:00Z", fingerprint, fingerprint, fingerprint, "scrub-" + id),
            fingerprint));
}
