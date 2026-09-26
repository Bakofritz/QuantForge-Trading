using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchOptimizationSelectionTests
{
    [Fact]
    public void Selection_requires_explicit_objective_and_complete_results()
    {
        var blocked = new ResearchOptimizationResult(
            new[] { new ResearchReport("job", ResearchResultStatus.DataBlocked, "dataset", "strategy", "timing", "params", "wf", "blocked", null, null) },
            false, "blocked");
        Assert.Throws<InvalidOperationException>(() => ResearchOptimizationSelectionRules.Select(blocked, ResearchOptimizationObjective.FinalEquity));
    }
}
