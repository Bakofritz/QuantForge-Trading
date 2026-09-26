using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core;

public sealed record WalkForwardSegment(
    string SegmentId,
    DateTimeOffset TrainingStart,
    DateTimeOffset TrainingEnd,
    DateTimeOffset EvaluationStart,
    DateTimeOffset EvaluationEnd,
    ResearchOptimizationApplicationRequest Optimization,
    ResearchOptimizationObjective Objective,
    ResearchRunRequest EvaluationRun);

public sealed record WalkForwardResearchPlan(IReadOnlyList<WalkForwardSegment> Segments);

public static class WalkForwardResearchPlanRules
{
    public static WalkForwardResearchPlan Create(IReadOnlyList<WalkForwardSegment> segments)
    {
        if (segments is null || segments.Count == 0)
            throw new InvalidOperationException("Walk-forward research requires at least one segment.");

        var ids = new HashSet<string>(StringComparer.Ordinal);
        DateTimeOffset? priorEvaluationEnd = null;
        foreach (var segment in segments)
        {
            if (string.IsNullOrWhiteSpace(segment.SegmentId) || !ids.Add(segment.SegmentId))
                throw new InvalidOperationException("Walk-forward segment identities must be complete and unique.");
            if (segment.TrainingEnd <= segment.TrainingStart || segment.EvaluationEnd <= segment.EvaluationStart)
                throw new InvalidOperationException("Walk-forward training and evaluation windows must have positive duration.");
            if (segment.TrainingEnd > segment.EvaluationStart)
                throw new InvalidOperationException("Walk-forward evaluation cannot overlap or precede its training window.");
            if (priorEvaluationEnd is not null && segment.EvaluationStart < priorEvaluationEnd.Value)
                throw new InvalidOperationException("Walk-forward evaluation windows must be chronological and non-overlapping.");

            var plan = ResearchOptimizationPlanRules.Create(segment.Optimization.Plan.Variants);
            var trainingPartitions = plan.Variants.Select(x => x.Job.Identity.TemporalPartitionId).Distinct(StringComparer.Ordinal).ToArray();
            if (trainingPartitions.Length != 1)
                throw new InvalidOperationException("One walk-forward training segment requires one explicit temporal partition identity.");
            if (string.Equals(trainingPartitions[0], segment.EvaluationRun.Job.Identity.TemporalPartitionId, StringComparison.Ordinal))
                throw new InvalidOperationException("Training and evaluation temporal partitions must be distinct.");

            var strategyFingerprints = plan.Variants.Select(x => x.Job.Identity.StrategyFingerprint).Distinct(StringComparer.Ordinal).ToArray();
            if (strategyFingerprints.Length != 1 ||
                !string.Equals(strategyFingerprints[0], segment.EvaluationRun.Job.Identity.StrategyFingerprint, StringComparison.Ordinal))
                throw new InvalidOperationException("Walk-forward optimization and evaluation must use one immutable strategy fingerprint.");

            foreach (var variant in plan.Variants)
                RequireWithinWindow(variant, segment.TrainingStart, segment.TrainingEnd, "training");
            RequireWithinWindow(segment.EvaluationRun, segment.EvaluationStart, segment.EvaluationEnd, "evaluation");
            priorEvaluationEnd = segment.EvaluationEnd;
        }

        return new WalkForwardResearchPlan(segments.ToArray());
    }

    private static void RequireWithinWindow(ResearchRunRequest run, DateTimeOffset start, DateTimeOffset end, string label)
    {
        if (run.MarketEvents is null || run.MarketEvents.Count == 0 || run.Intents is null || run.Intents.Count == 0)
            throw new InvalidOperationException($"Walk-forward {label} runs require market events and simulation intents.");
        if (run.MarketEvents.Any(x => x.Timestamp < start || x.Timestamp >= end))
            throw new InvalidOperationException($"Walk-forward {label} market events must remain inside the declared temporal window.");
        if (run.Intents.Any(x => x.SignalTime < start || x.SignalTime >= end || x.EarliestFillTime < start || x.EarliestFillTime >= end))
            throw new InvalidOperationException($"Walk-forward {label} intents must remain inside the declared temporal window.");
    }
}

public sealed record WalkForwardSegmentResult(
    string SegmentId,
    ResearchOptimizationApplicationResult Optimization,
    ResearchReport? Evaluation,
    string SegmentFingerprint);

public sealed record WalkForwardResearchResult(
    IReadOnlyList<WalkForwardSegmentResult> Segments,
    bool Complete,
    string? BlockReason,
    string ResultFingerprint);

public static class WalkForwardResearchRunner
{
    public static WalkForwardResearchResult Run(WalkForwardResearchPlan input)
    {
        var plan = WalkForwardResearchPlanRules.Create(input.Segments);
        var results = new List<WalkForwardSegmentResult>(plan.Segments.Count);
        string? blockReason = null;

        foreach (var segment in plan.Segments)
        {
            var optimization = ResearchOptimizationApplicationWorkflow.Run(segment.Optimization, segment.Objective);
            if (!optimization.Evaluation.Complete || optimization.Selection is null)
            {
                blockReason = $"Walk-forward segment {segment.SegmentId} optimization did not produce a complete deterministic selection.";
                results.Add(new WalkForwardSegmentResult(segment.SegmentId, optimization, null,
                    FingerprintSegment(segment.SegmentId, optimization.ResultFingerprint, "blocked")));
                break;
            }

            var selectedVariant = segment.Optimization.Plan.Variants.Single(x =>
                string.Equals(x.Job.Identity.JobFingerprint, optimization.Selection.SelectedJobId, StringComparison.Ordinal));
            var evaluationIdentity = segment.EvaluationRun.Job.Identity;
            if (!string.Equals(selectedVariant.Job.Identity.StrategyFingerprint, evaluationIdentity.StrategyFingerprint, StringComparison.Ordinal) ||
                !string.Equals(selectedVariant.Job.Identity.ParameterSetFingerprint, evaluationIdentity.ParameterSetFingerprint, StringComparison.Ordinal))
                throw new InvalidOperationException("Walk-forward evaluation must carry the selected training strategy and parameter identities unchanged.");

            var workflow = new ResearchWorkflowRequest(
                new ResearchBatchRequest(ResearchBatchMode.ReadOnlyResearch, new[] { segment.EvaluationRun }),
                segment.Optimization.Reliability,
                segment.Optimization.SessionCoverage);
            var evaluationResult = EndToEndResearchCoordinator.RunResearchAdmitted(new EndToEndResearchRequest(
                workflow,
                segment.Optimization.DatasetCatalog,
                segment.Optimization.StrategyAdmissions));
            var evaluation = evaluationResult.Reports.Single();
            var segmentFingerprint = FingerprintSegment(
                segment.SegmentId,
                optimization.ResultFingerprint,
                evaluation.EvidenceTail?.Fingerprint ?? $"{evaluation.Status}:{evaluation.BlockReason}");
            results.Add(new WalkForwardSegmentResult(segment.SegmentId, optimization, evaluation, segmentFingerprint));

            if (evaluation.Status != ResearchResultStatus.Complete)
            {
                blockReason = $"Walk-forward segment {segment.SegmentId} evaluation ended as {evaluation.Status}: {evaluation.BlockReason}";
                break;
            }
        }

        var complete = blockReason is null && results.Count == plan.Segments.Count;
        var payload = string.Join("\n", results.Select(x => $"{x.SegmentId}|{x.SegmentFingerprint}"));
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{complete}|{blockReason}\n{payload}")));
        return new WalkForwardResearchResult(results, complete, blockReason, fingerprint);
    }

    private static string FingerprintSegment(string segmentId, string optimizationFingerprint, string evaluationFingerprint) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{segmentId}|{optimizationFingerprint}|{evaluationFingerprint}")));
}

public static class WalkForwardResearchResultRules
{
    public static void Validate(WalkForwardResearchResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.Segments is null || result.Segments.Count == 0)
            throw new InvalidOperationException("Walk-forward result requires at least one executed segment.");
        if (string.IsNullOrWhiteSpace(result.ResultFingerprint))
            throw new InvalidOperationException("Walk-forward result fingerprint is required.");
        if (result.Segments.Select(x => x.SegmentId).Distinct(StringComparer.Ordinal).Count() != result.Segments.Count)
            throw new InvalidOperationException("Walk-forward result contains duplicate segment identities.");

        foreach (var segment in result.Segments)
        {
            if (string.IsNullOrWhiteSpace(segment.SegmentId) || string.IsNullOrWhiteSpace(segment.SegmentFingerprint))
                throw new InvalidOperationException("Walk-forward segment result identity is incomplete.");
            _ = ProductUiOptimizationResultPresenter.Create(segment.Optimization);
            if (segment.Evaluation is { } evaluation)
                ResearchReportRules.Validate(evaluation);
            var evaluationFingerprint = segment.Evaluation?.EvidenceTail?.Fingerprint ??
                (segment.Evaluation is { } report ? $"{report.Status}:{report.BlockReason}" : "blocked");
            var expectedSegment = ComputeSegmentFingerprint(segment.SegmentId, segment.Optimization.ResultFingerprint, evaluationFingerprint);
            if (!string.Equals(segment.SegmentFingerprint, expectedSegment, StringComparison.Ordinal))
                throw new InvalidOperationException("Walk-forward segment fingerprint verification failed.");
        }

        if (result.Complete)
        {
            if (result.BlockReason is not null || result.Segments.Any(x =>
                    !x.Optimization.Evaluation.Complete || x.Optimization.Selection is null ||
                    x.Evaluation is null || x.Evaluation.Value.Status != ResearchResultStatus.Complete))
                throw new InvalidOperationException("Complete walk-forward results require complete optimization and evaluation for every segment.");
        }
        else if (string.IsNullOrWhiteSpace(result.BlockReason))
        {
            throw new InvalidOperationException("Incomplete walk-forward results require an explicit block reason.");
        }

        var payload = string.Join("\n", result.Segments.Select(x => $"{x.SegmentId}|{x.SegmentFingerprint}"));
        var expectedResult = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{result.Complete}|{result.BlockReason}\n{payload}")));
        if (!string.Equals(result.ResultFingerprint, expectedResult, StringComparison.Ordinal))
            throw new InvalidOperationException("Walk-forward result fingerprint verification failed.");
    }

    private static string ComputeSegmentFingerprint(string segmentId, string optimizationFingerprint, string evaluationFingerprint) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{segmentId}|{optimizationFingerprint}|{evaluationFingerprint}")));
}
