namespace QuantForge.Core;

public enum AuthorityDomain
{
    HistoricalResearch,
    SimulatedReplay,
    SimulatedAccount,
    ReadOnlyResearch,
    LiveAccount
}

public readonly record struct ResearchAuthorityDecision(
    AuthorityDomain Domain,
    bool CanSubmitOrders,
    bool CanChangeApplicationSettings);

public static class AuthorityBoundary
{
    public static ResearchAuthorityDecision EvaluateResearch(AuthorityDomain domain)
    {
        if (domain == AuthorityDomain.LiveAccount)
            throw new InvalidOperationException("Live-account authority is outside the research runtime.");

        return new ResearchAuthorityDecision(domain, false, false);
    }
}