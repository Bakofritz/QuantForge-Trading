using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchOptimizationRunnerTests
{
    [Fact]
    public void Optimization_requires_all_variants_to_remain_within_the_read_only_result_pipeline()
    {
        var report = new ResearchReport("job", ResearchResultStatus.DataBlocked, "dataset", "strategy", "timing", "params", "wf", "blocked", null, null);
        var result = new ResearchOptimizationResult(new[] { report }, false, "blocked");
        Assert.False(result.Complete);
        Assert.Equal(ResearchResultStatus.DataBlocked, result.Reports[0].Status);
    }
}
