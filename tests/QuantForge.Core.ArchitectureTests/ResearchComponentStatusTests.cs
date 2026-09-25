using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public class ResearchComponentStatusTests
{
    [Fact]
    public void Invalid_report_propagates_explicit_terminal_reason()
    {
        var report = new ResearchReport("job", ResearchResultStatus.Invalid, "data", "strategy", "exec", "params", "partition", "unsafe admission", null, null);
        var status = ResearchComponentStatusRules.FromReport(report);

        Assert.Equal(ResearchComponentState.Invalid, status.State);
        Assert.Equal("unsafe admission", status.Message);
        ResearchComponentStatusRules.RequireTerminal(status);
    }

    [Fact]
    public void Blocked_report_propagates_explicit_terminal_reason()
    {
        var report = new ResearchReport("job", ResearchResultStatus.DataBlocked, "data", "strategy", "exec", "params", "partition", "data gap", null, null);
        var status = ResearchComponentStatusRules.FromReport(report);

        Assert.Equal(ResearchComponentState.DataBlocked, status.State);
        Assert.Equal("data gap", status.Message);
    }

    [Fact]
    public void Nonterminal_component_cannot_be_presented_as_finished()
    {
        var status = new ResearchComponentStatus("job", ResearchComponentState.Running, null);
        Assert.Throws<InvalidOperationException>(() => ResearchComponentStatusRules.RequireTerminal(status));
    }
}
