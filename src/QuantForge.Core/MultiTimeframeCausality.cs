namespace QuantForge.Core;

public readonly record struct TimeframeObservation(
    string Timeframe,
    DateTimeOffset BarOpenTime,
    DateTimeOffset BarCloseTime,
    DateTimeOffset AvailableTime);

public sealed record ResearchTimeframeContext(
    string JobFingerprint,
    DateTimeOffset DecisionTime,
    IReadOnlyList<TimeframeObservation> Observations);

public static class ResearchTimeframeContextRules
{
    public static void Validate(ResearchTimeframeContext context)
    {
        if (string.IsNullOrWhiteSpace(context.JobFingerprint))
            throw new InvalidOperationException("Multi-timeframe context requires research job identity.");

        if (context.Observations is null || context.Observations.Count == 0)
            throw new InvalidOperationException("Multi-timeframe context requires at least one closed observation.");

        var timeframes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var observation in context.Observations)
        {
            if (string.IsNullOrWhiteSpace(observation.Timeframe) || !timeframes.Add(observation.Timeframe))
                throw new InvalidOperationException("Multi-timeframe context requires unique, non-empty timeframe identities.");

            if (observation.BarCloseTime < observation.BarOpenTime)
                throw new InvalidOperationException("Timeframe observation cannot close before it opens.");

            if (observation.AvailableTime < observation.BarCloseTime)
                throw new InvalidOperationException("A closed-bar observation cannot be available before the bar closes.");

            CausalIntegrity.RequireClosedBar(context.DecisionTime, observation.BarCloseTime);
            CausalIntegrity.RequireInformationAvailableAtObservation(context.DecisionTime, observation.AvailableTime);
        }
    }
}
