namespace QuantForge.Core;

public sealed record ResearchRunRequest(
    ResearchJobSpec Job,
    IReadOnlyList<SimulationIntent> Intents,
    IReadOnlyList<MarketEvent> MarketEvents,
    decimal StartingCash,
    decimal CommissionPerUnit,
    decimal SlippagePerUnit);

public static class DeterministicResearchRunner
{
    public static ResearchReport Run(ResearchRunRequest request)
    {
        if (request.Intents is null || request.MarketEvents is null)
            throw new InvalidOperationException("Research runner requires intents and market events.");

        if (request.Intents.Count == 0)
            throw new InvalidOperationException("A research run requires at least one simulation intent.");

        if (request.StartingCash < 0 ||
            request.CommissionPerUnit < 0 ||
            request.SlippagePerUnit < 0)
            throw new InvalidOperationException("Simulation economics must be non-negative.");

        try
        {
            ResearchJobRules.RequireRunnable(request.Job);
        }
        catch (InvalidOperationException ex)
        {
            return InvalidReport(request.Job.Identity, ex.Message);
        }

        RequireIntentCompatibility(request);
        var events = ValidateAndOrderEvents(request.MarketEvents);

        var fills = new List<SimulationFill>(request.Intents.Count);
        var nextEventIndex = 0;
        var orderedIntents = request.Intents
            .Select((intent, index) => (intent, index))
            .OrderBy(x => x.intent.SignalTime)
            .ThenBy(x => x.intent.EarliestFillTime)
            .ThenBy(x => x.index)
            .ToArray();

        foreach (var item in orderedIntents)
        {
            var intent = item.intent;
            var found = false;
            SimulationFill fill = default;

            for (var eventIndex = nextEventIndex; eventIndex < events.Count; eventIndex++)
            {
                var market = events[eventIndex];
                if (market.Timestamp >= intent.EarliestFillTime)
                {
                    fill = DeterministicFillModel.FillAtNextEligibleEvent(
                        intent,
                        market,
                        request.CommissionPerUnit,
                        request.SlippagePerUnit);
                    found = true;
                    nextEventIndex = eventIndex + 1;
                    break;
                }
            }

            if (!found)
            {
                return DataBlockedReport(
                    request.Job.Identity,
                    $"No eligible market event exists at or after {intent.EarliestFillTime:O} after prior execution events.");
            }

            fills.Add(fill);
        }

        var account = new SimulatedAccount(
            request.Intents[0].LedgerNamespace,
            request.StartingCash);

        var evidence = ResearchEvidence.CreateRoot(request.Job.Identity);
        long evidenceSequence = 0;

        foreach (var fill in fills
            .Select((fill, index) => (fill, index))
            .OrderBy(x => x.fill.FillTime)
            .ThenBy(x => x.index))
        {
            account.ApplyFill(fill.fill);
            evidenceSequence++;
            evidence = ResearchEvidence.AppendFill(evidence, evidenceSequence, fill.fill);
        }

        var finalSnapshot = account.Snapshot(events[^1].Close);

        var report = new ResearchReport(
            request.Job.Identity.JobFingerprint,
            ResearchResultStatus.Complete,
            request.Job.Identity.DatasetFingerprint,
            request.Job.Identity.StrategyFingerprint,
            request.Job.Identity.ExecutionPolicyFingerprint,
            request.Job.Identity.ParameterSetFingerprint,
            request.Job.Identity.TemporalPartitionId,
            null,
            finalSnapshot,
            evidence);

        ResearchReportRules.Validate(report);
        return report;
    }

    public static IReadOnlyList<ResearchReport> RunBatch(
        IReadOnlyList<ResearchRunRequest> requests)
    {
        if (requests is null || requests.Count == 0)
            throw new InvalidOperationException("A research batch requires at least one run.");

        var strategies = requests.Select(request =>
        {
            if (request.Intents is null || request.Intents.Count == 0)
                throw new InvalidOperationException("Every batch run requires at least one simulation intent.");

            var first = request.Intents[0];
            var parts = first.LedgerNamespace.Split('|');
            if (parts.Length != 3)
                throw new InvalidOperationException(
                    "Batch ledger namespaces must use batch|strategy|account identity.");

            return new ResearchStrategyInstance(
                request.Job.Strategy.StrategyId,
                request.Job.Strategy.SourceFingerprint,
                new LedgerNamespace(parts[0], parts[1], parts[2]));
        }).ToArray();

        ResearchBatchRules.RequireIndependentStrategies(strategies);
        return requests.Select(Run).ToArray();
    }

    private static void RequireIntentCompatibility(ResearchRunRequest request)
    {
        var first = request.Intents[0];

        foreach (var intent in request.Intents)
        {
            intent.ValidateResearchOnly();

            if (intent.StrategyId != request.Job.Strategy.StrategyId)
                throw new InvalidOperationException(
                    "Simulation intent strategy identity does not match the admitted strategy.");

            if (intent.LedgerNamespace != first.LedgerNamespace)
                throw new InvalidOperationException(
                    "All intents in a research run must share one isolated ledger namespace.");
        }

        if (string.IsNullOrWhiteSpace(first.LedgerNamespace))
            throw new InvalidOperationException("Simulation intents require an isolated ledger namespace.");
    }

    private static IReadOnlyList<MarketEvent> ValidateAndOrderEvents(
        IReadOnlyList<MarketEvent> marketEvents)
    {
        if (marketEvents.Count == 0)
            throw new InvalidOperationException("An admitted research run requires market events.");

        var ordered = marketEvents
            .OrderBy(x => x.Sequence)
            .ThenBy(x => x.Timestamp)
            .ToArray();

        for (var i = 0; i < ordered.Length; i++)
        {
            ordered[i].Validate();

            if (i > 0)
            {
                if (ordered[i].Sequence <= ordered[i - 1].Sequence)
                    throw new InvalidOperationException(
                        "Market-event sequence must be strictly increasing.");

                if (ordered[i].Timestamp < ordered[i - 1].Timestamp)
                    throw new InvalidOperationException(
                        "Market-event timestamps must be non-decreasing.");
            }
        }

        return ordered;
    }

    private static ResearchReport DataBlockedReport(
        ResearchJobIdentity identity,
        string reason)
    {
        var report = new ResearchReport(
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

        ResearchReportRules.Validate(report);
        return report;
    }

    private static ResearchReport InvalidReport(
        ResearchJobIdentity identity,
        string reason)
    {
        var report = new ResearchReport(
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

        ResearchReportRules.Validate(report);
        return report;
    }
}
