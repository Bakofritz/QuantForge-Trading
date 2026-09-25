using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ProductUiJobLifecycleRulesTests
{
    [Fact]
    public void PendingJob_CanEnterRunningState()
    {
        var pending = ProductUiJobLifecycleRules.Pending("job-a");

        var running = ProductUiJobLifecycleRules.Start(pending);

        Assert.Equal(ProductUiJobState.Running, running.State);
        Assert.False(running.CanRetry);
    }

    [Fact]
    public void BlockedTerminalState_CanResetToPendingForExplicitRetry()
    {
        var terminal = new ResearchComponentStatus(
            "job-a",
            ResearchComponentState.DataBlocked,
            "Dataset has an unresolved gap.");

        var blocked = ProductUiJobLifecycleRules.FromTerminalStatus(terminal);
        var retry = ProductUiJobLifecycleRules.Retry(blocked);

        Assert.Equal(ProductUiJobState.DataBlocked, blocked.State);
        Assert.True(blocked.CanRetry);
        Assert.Equal(ProductUiJobState.Pending, retry.State);
        Assert.Equal("job-a", retry.JobFingerprint);
        Assert.Null(retry.Message);
    }

    [Fact]
    public void CompleteTerminalState_CannotBeRetried()
    {
        var terminal = new ResearchComponentStatus(
            "job-a",
            ResearchComponentState.Complete,
            null);

        var complete = ProductUiJobLifecycleRules.FromTerminalStatus(terminal);

        Assert.False(complete.CanRetry);
        Assert.Throws<InvalidOperationException>(() =>
            ProductUiJobLifecycleRules.Retry(complete));
    }

    [Fact]
    public void TerminalJob_CannotBeStartedWithoutExplicitRetryReset()
    {
        var terminal = new ResearchComponentStatus(
            "job-a",
            ResearchComponentState.Invalid,
            "Strategy admission failed.");
        var invalid = ProductUiJobLifecycleRules.FromTerminalStatus(terminal);

        Assert.Throws<InvalidOperationException>(() =>
            ProductUiJobLifecycleRules.Start(invalid));
    }
}
