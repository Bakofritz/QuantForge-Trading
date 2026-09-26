using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class WalkForwardResearchRunnerTests
{
    [Fact]
    public void Runner_selects_on_training_and_evaluates_selected_identity_out_of_sample()
    {
        var segment = WalkForwardResearchPlanTests.FixtureSegment();
        var plan = WalkForwardResearchPlanRules.Create(new[] { segment });

        var result = WalkForwardResearchRunner.Run(plan);

        Assert.True(result.Complete);
        var completed = Assert.Single(result.Segments);
        Assert.Equal("job-a", completed.Optimization.Selection!.SelectedJobId);
        Assert.NotNull(completed.Evaluation);
        Assert.Equal("params-a", completed.Evaluation!.Value.ParameterFingerprint);
        Assert.Equal("eval-01", completed.Evaluation.Value.TemporalPartition);
        Assert.Equal(ResearchResultStatus.Complete, completed.Evaluation.Value.Status);
    }

    [Fact]
    public void Runner_rejects_evaluation_parameter_identity_not_selected_in_training()
    {
        var segment = WalkForwardResearchPlanTests.FixtureSegment();
        var mismatched = segment.EvaluationRun with
        {
            Job = segment.EvaluationRun.Job with
            {
                Identity = segment.EvaluationRun.Job.Identity with { ParameterSetFingerprint = "params-b" }
            }
        };
        var plan = WalkForwardResearchPlanRules.Create(new[] { segment with { EvaluationRun = mismatched } });

        Assert.Throws<InvalidOperationException>(() => WalkForwardResearchRunner.Run(plan));
    }
}
