namespace QuantForge.Core;

public enum ExecutionTimingPolicy
{
    NextBarOpen,
    NextEligibleTick,
    ExplicitCloseAuction
}

public static class ExecutionTimingPolicyRules
{
    public static void RequireValidSameBarModel(
        bool signalAndFillShareBar,
        ExecutionTimingPolicy policy)
    {
        if (signalAndFillShareBar && policy != ExecutionTimingPolicy.ExplicitCloseAuction)
            throw new InvalidOperationException(
                "Same-bar execution requires an explicit close-auction model.");
    }
}
