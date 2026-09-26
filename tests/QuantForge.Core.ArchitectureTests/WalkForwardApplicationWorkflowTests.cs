using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class WalkForwardApplicationWorkflowTests
{
    [Fact]
    public void Application_workflow_runs_persists_and_recovers_same_verified_result()
    {
        var plan = WalkForwardResearchPlanRules.Create(new[] { WalkForwardResearchPlanTests.FixtureSegment() });
        var root = Path.Combine(Path.GetTempPath(), "qf-wf-app-" + Guid.NewGuid().ToString("N"));
        try
        {
            var created = WalkForwardApplicationWorkflow.RunAndPersist(plan, root);
            var recovered = WalkForwardApplicationWorkflow.Recover(root, created.Research.ResultFingerprint);

            Assert.True(File.Exists(created.PersistedPath));
            Assert.Equal(created.Research.ResultFingerprint, recovered.Research.ResultFingerprint);
            Assert.Equal(created.State, recovered.State);
            Assert.False(recovered.State.LiveAccountEnabled);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}
