namespace QuantForge.Core;

public sealed record ResearchWorkflowRequest(
    ResearchBatchRequest Batch,
    IReadOnlyList<DataReliabilityAssessment> Reliability,
    IReadOnlyList<SessionCoverageReport>? SessionCoverage = null,
    IReadOnlyList<ResearchTimeframeContext>? TimeframeContexts = null);

public sealed record ResearchWorkflowResult(
    IReadOnlyList<ResearchReport> Reports,
    IReadOnlyList<ResearchComponentStatus> ComponentStatuses,
    IReadOnlyList<DataReliabilityAssessment> Reliability);

public static class ResearchWorkflowCoordinator
{
    public static ResearchWorkflowResult Run(ResearchWorkflowRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Batch.Runs is null || request.Batch.Runs.Count == 0)
            throw new InvalidOperationException("A research workflow requires at least one run.");

        if (request.Reliability is null)
            throw new InvalidOperationException("A research workflow requires data reliability assessments.");

        var reliabilityByDataset = new Dictionary<string, DataReliabilityAssessment>(StringComparer.Ordinal);
        foreach (var assessment in request.Reliability)
        {
            DataReliabilityRules.Validate(assessment);
            if (!reliabilityByDataset.TryAdd(assessment.DatasetFingerprint, assessment))
                throw new InvalidOperationException("A research workflow cannot contain duplicate reliability assessments for one dataset.");
        }

        var coverageByDataset = new Dictionary<string, SessionCoverageReport>(StringComparer.Ordinal);
        if (request.SessionCoverage is not null)
        {
            foreach (var coverage in request.SessionCoverage)
            {
                if (!coverageByDataset.TryAdd(coverage.DatasetFingerprint, coverage))
                    throw new InvalidOperationException("A research workflow cannot contain duplicate session coverage reports for one dataset.");
            }
        }

        var timeframeByJob = new Dictionary<string, ResearchTimeframeContext>(StringComparer.Ordinal);
        if (request.TimeframeContexts is not null)
        {
            foreach (var context in request.TimeframeContexts)
            {
                if (string.IsNullOrWhiteSpace(context.JobFingerprint))
                    throw new InvalidOperationException("Multi-timeframe workflow context requires research job identity.");

                if (!timeframeByJob.TryAdd(context.JobFingerprint, context))
                    throw new InvalidOperationException("A research workflow cannot contain duplicate multi-timeframe contexts for one job.");
            }
        }

        var jobIds = new HashSet<string>(StringComparer.Ordinal);
        var reports = new List<ResearchReport>(request.Batch.Runs.Count);
        var statuses = new List<ResearchComponentStatus>(request.Batch.Runs.Count);

        foreach (var run in request.Batch.Runs)
        {
            var identity = run.Job.Identity;
            ResearchReport report;

            if (string.IsNullOrWhiteSpace(identity.JobFingerprint) || !jobIds.Add(identity.JobFingerprint))
            {
                report = Invalid(identity, "Research workflow job fingerprints must be complete and unique.");
            }
            else if (!string.Equals(run.Job.Data.DatasetFingerprint, identity.DatasetFingerprint, StringComparison.Ordinal))
            {
                report = Invalid(identity, "Research job data identity does not match the reproducibility dataset fingerprint.");
            }
            else if (timeframeByJob.TryGetValue(identity.JobFingerprint, out var timeframeContext) &&
                     !TryValidateTimeframeContext(timeframeContext, out var timeframeReason))
            {
                report = Invalid(identity, $"Multi-timeframe causal validation failed: {timeframeReason}");
            }
            else if (request.SessionCoverage is not null &&
                     (!coverageByDataset.TryGetValue(identity.DatasetFingerprint, out var coverage) || !coverage.ResearchAdmissible))
            {
                report = DataBlocked(identity, "Authoritative session coverage is incomplete or unavailable for the admitted dataset.");
            }
            else if (!reliabilityByDataset.TryGetValue(identity.DatasetFingerprint, out var reliability))
            {
                report = DataBlocked(identity, "No data reliability assessment exists for the admitted dataset.");
            }
            else if (!DataReliabilityRules.IsResearchAdmissible(reliability))
            {
                report = DataBlocked(
                    identity,
                    ReliabilityBlockReason(reliability));
            }
            else
            {
                try
                {
                    report = ResearchResultPipeline.Run(run).Report;
                }
                catch (InvalidOperationException ex)
                {
                    report = Invalid(identity, $"Contained research workflow admission failure: {ex.Message}");
                }
            }

            ResearchReportRules.Validate(report);
            reports.Add(report);

            var status = ResearchComponentStatusRules.FromReport(report);
            ResearchComponentStatusRules.RequireTerminal(status);
            statuses.Add(status);
        }

        return new ResearchWorkflowResult(reports, statuses, request.Reliability.ToArray());
    }

    private static bool TryValidateTimeframeContext(
        ResearchTimeframeContext context,
        out string? reason)
    {
        try
        {
            ResearchTimeframeContextRules.Validate(context);
            reason = null;
            return true;
        }
        catch (InvalidOperationException ex)
        {
            reason = ex.Message;
            return false;
        }
    }

    private static string ReliabilityBlockReason(DataReliabilityAssessment reliability)
    {
        var reasons = new List<string>();
        if (!reliability.ComparedToLiveBenchmark)
            reasons.Add("live benchmark comparison is missing");
        if (reliability.UnresolvedGapCount > 0)
            reasons.Add($"{reliability.UnresolvedGapCount} unresolved data gap(s)");
        if (reliability.ConflictingOverlapCount > 0)
            reasons.Add($"{reliability.ConflictingOverlapCount} conflicting overlap(s)");

        var detail = reasons.Count == 0 ? "data reliability is not research-admissible" : string.Join(", ", reasons);
        return string.IsNullOrWhiteSpace(reliability.Limitation)
            ? $"Data reliability blocked research: {detail}."
            : $"Data reliability blocked research: {detail}. Limitation: {reliability.Limitation}";
    }

    private static ResearchReport DataBlocked(ResearchJobIdentity identity, string reason) =>
        new(
            identity.JobFingerprint,
            ResearchResultStatus.DataBlocked,
            identity.DatasetFingerprint,
            identity.StrategyFingerprint,
            identity.ExecutionPolicyFingerprint,
            identity.ParameterSetFingerprint,
            identity.TemporalPartitionId,
            reason,
            null,
            null);

    private static ResearchReport Invalid(ResearchJobIdentity identity, string reason) =>
        new(
            identity.JobFingerprint,
            ResearchResultStatus.Invalid,
            identity.DatasetFingerprint,
            identity.StrategyFingerprint,
            identity.ExecutionPolicyFingerprint,
            identity.ParameterSetFingerprint,
            identity.TemporalPartitionId,
            reason,
            null,
            null);
}
