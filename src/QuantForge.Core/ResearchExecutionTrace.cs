namespace QuantForge.Core;

public readonly record struct ResearchEquityPoint(
    DateTimeOffset Timestamp,
    decimal Equity,
    decimal Cash,
    decimal PositionQuantity,
    decimal MarketPrice);

public readonly record struct ResearchLedgerRecord(
    string LedgerNamespace,
    string StrategyId,
    DateTimeOffset Timestamp,
    string EventType,
    decimal CashDelta,
    decimal RealizedPnl,
    string EvidenceFingerprint);

public sealed record ResearchExecutionTrace(
    IReadOnlyList<SimulationFill> Fills,
    IReadOnlyList<ResearchEquityPoint> EquityCurve,
    IReadOnlyList<ResearchLedgerRecord> Ledger,
    string TraceFingerprint);

public static class ResearchExecutionTraceFactory
{
    public static ResearchExecutionTrace Create(
        ResearchJobIdentity identity,
        IReadOnlyList<SimulationFill> fills,
        IReadOnlyList<MarketEvent> events,
        decimal startingCash)
    {
        if (string.IsNullOrWhiteSpace(identity.JobFingerprint))
            throw new InvalidOperationException("Research job identity is required for an execution trace.");
        if (fills is null || fills.Count == 0)
            throw new InvalidOperationException("Execution traces require at least one fill.");
        if (events is null || events.Count == 0)
            throw new InvalidOperationException("Execution traces require market events.");

        var account = new SimulatedAccount(fills[0].LedgerNamespace, startingCash);
        var curve = new List<ResearchEquityPoint>();
        var ledger = new List<ResearchLedgerRecord>();
        var evidence = ResearchEvidence.CreateRoot(identity);
        long sequence = 0;

        foreach (var fill in fills.Select((value, index) => (value, index)).OrderBy(x => x.value.FillTime).ThenBy(x => x.index))
        {
            var before = account.Snapshot(fill.Price);
            var after = account.ApplyFill(fill);
            sequence++;
            evidence = ResearchEvidence.AppendFill(evidence, sequence, fill);

            curve.Add(new ResearchEquityPoint(
                fill.FillTime,
                after.Equity,
                after.Cash,
                after.Position.Quantity,
                after.MarketPrice));

            var cashDelta = after.Cash - before.Cash;
            ledger.Add(new ResearchLedgerRecord(
                fill.LedgerNamespace,
                fill.StrategyId,
                fill.FillTime,
                fill.Side == SimulationSide.Buy ? "simulation-buy" : "simulation-sell",
                cashDelta,
                after.RealizedPnl - before.RealizedPnl,
                evidence.Fingerprint));
        }

        var fingerprint = EvidenceChain.Sha256(string.Join("\n", new[]
        {
            identity.JobFingerprint,
            identity.DatasetFingerprint,
            identity.StrategyFingerprint,
            identity.ExecutionPolicyFingerprint,
            identity.ParameterSetFingerprint,
            identity.TemporalPartitionId,
            string.Join("|", curve.Select(x => $"{x.Timestamp:O}:{x.Equity}:{x.Cash}:{x.PositionQuantity}:{x.MarketPrice}")),
            string.Join("|", ledger.Select(x => $"{x.Timestamp:O}:{x.EventType}:{x.CashDelta}:{x.RealizedPnl}:{x.EvidenceFingerprint}"))
        }));

        return new ResearchExecutionTrace(fills.ToArray(), curve.ToArray(), ledger.ToArray(), fingerprint);
    }
}

public static class ResearchExecutionTraceRules
{
    public static void Validate(ResearchExecutionTrace trace)
    {
        if (trace.Fills is null || trace.Fills.Count == 0 ||
            trace.EquityCurve is null || trace.EquityCurve.Count != trace.Fills.Count ||
            trace.Ledger is null || trace.Ledger.Count != trace.Fills.Count ||
            string.IsNullOrWhiteSpace(trace.TraceFingerprint))
            throw new InvalidOperationException("Research execution trace is incomplete.");

        for (var i = 0; i < trace.Fills.Count; i++)
        {
            trace.Fills[i].StrategyId.ToString();
            if (trace.Fills[i].Quantity <= 0 || trace.Fills[i].FillTime != trace.EquityCurve[i].Timestamp)
                throw new InvalidOperationException("Research execution trace contains invalid fill/curve alignment.");
            if (string.IsNullOrWhiteSpace(trace.Ledger[i].EvidenceFingerprint) ||
                trace.Ledger[i].LedgerNamespace != trace.Fills[i].LedgerNamespace ||
                trace.Ledger[i].StrategyId != trace.Fills[i].StrategyId)
                throw new InvalidOperationException("Research execution trace ledger identity is invalid.");
        }
    }
}
