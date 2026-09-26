using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ProductWalkForwardSessionTests
{
    [Fact]
    public void Session_clears_previous_state_when_recovery_becomes_invalid()
    {
        var plan = WalkForwardResearchPlanRules.Create(new[] { WalkForwardResearchPlanTests.FixtureSegment() });
        var root = Path.Combine(Path.GetTempPath(), "qf-wf-session-" + Guid.NewGuid().ToString("N"));
        try
        {
            var created = WalkForwardApplicationWorkflow.RunAndPersist(plan, root);
            var session = new ProductWalkForwardSession();
            session.Load(root, created.Research.ResultFingerprint);
            Assert.Equal(ProductWalkForwardSessionStatus.Ready, session.Status);
            Assert.NotNull(session.State);

            File.AppendAllText(Path.Combine(Path.GetDirectoryName(created.PersistedPath)!, "walk-forward.md"), "tampered\n");
            session.Load(root, created.Research.ResultFingerprint);
            Assert.Equal(ProductWalkForwardSessionStatus.Invalid, session.Status);
            Assert.Null(session.State);
            Assert.Equal("QF-WF-PRESENTATION-REJECTED", session.DiagnosticCode);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}
