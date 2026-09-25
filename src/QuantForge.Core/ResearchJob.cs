namespace QuantForge.Core;

public readonly record struct ResearchJobSpec(
    AuthorityDomain Authority,
    DataAdmission Data,
    StrategyCapabilityManifest Strategy,
    ResearchJobIdentity Identity,
    ExecutionTimingPolicy ExecutionTiming,
    bool SignalAndFillShareBar);

public static class ResearchJobRules
{
    public static void RequireRunnable(ResearchJobSpec job)
    {
        _ = AuthorityBoundary.EvaluateResearch(job.Authority);
        DataAdmissionRules.RequireAdmitted(job.Data);
        StrategyAdmissionRules.RequireResearchSafe(job.Strategy);
        ResearchJobIdentityRules.RequireComplete(job.Identity);
        ExecutionTimingPolicyRules.RequireValidSameBarModel(
            job.SignalAndFillShareBar,
            job.ExecutionTiming);
    }
}
