namespace QuantForge.Core;

public enum ProductUiJobState
{
    Pending,
    Running,
    Complete,
    DataBlocked,
    Invalid
}

public sealed record ProductUiJobLifecycleState(
    string JobFingerprint,
    ProductUiJobState State,
    string? Message,
    bool CanRetry);

public static class ProductUiJobLifecycleRules
{
    public static ProductUiJobLifecycleState Pending(string jobFingerprint)
    {
        RequireIdentity(jobFingerprint);
        return new ProductUiJobLifecycleState(jobFingerprint, ProductUiJobState.Pending, null, false);
    }

    public static ProductUiJobLifecycleState Start(ProductUiJobLifecycleState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        RequireIdentity(state.JobFingerprint);

        if (state.State != ProductUiJobState.Pending)
            throw new InvalidOperationException("Only a pending product UI research job can enter the running state.");

        return state with { State = ProductUiJobState.Running, Message = null, CanRetry = false };
    }

    public static ProductUiJobLifecycleState FromTerminalStatus(ResearchComponentStatus status)
    {
        ArgumentNullException.ThrowIfNull(status);
        ResearchComponentStatusRules.RequireTerminal(status);

        return status.State switch
        {
            ResearchComponentState.Complete =>
                new(status.JobFingerprint, ProductUiJobState.Complete, null, false),
            ResearchComponentState.DataBlocked =>
                new(status.JobFingerprint, ProductUiJobState.DataBlocked, status.Message, true),
            ResearchComponentState.Invalid =>
                new(status.JobFingerprint, ProductUiJobState.Invalid, status.Message, true),
            _ => throw new InvalidOperationException("Product UI terminal state mapping received a non-terminal research state.")
        };
    }

    public static ProductUiJobLifecycleState Retry(ProductUiJobLifecycleState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        RequireIdentity(state.JobFingerprint);

        if (!state.CanRetry || state.State is not (ProductUiJobState.DataBlocked or ProductUiJobState.Invalid))
            throw new InvalidOperationException("Only blocked or invalid product UI research jobs can be reset for retry.");

        return Pending(state.JobFingerprint);
    }

    private static void RequireIdentity(string jobFingerprint)
    {
        if (string.IsNullOrWhiteSpace(jobFingerprint))
            throw new InvalidOperationException("Product UI job lifecycle requires research job identity.");
    }
}
