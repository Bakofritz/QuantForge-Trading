using QuantForge.Core;
using Xunit;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchOutcomeArtifactFileStoreTests
{
    [Fact]
    public void Outcome_package_round_trips_with_launch_and_gate_validation()
    {
        var gates = TestFixtures.Gates();
        var intent = ResearchLaunchIntentRules.Create(gates, TestFixtures.Job(gates));
        var id = intent.Job.Identity;
        var report = new ResearchReport(id.JobFingerprint, ResearchResultStatus.DataBlocked, id.DatasetFingerprint,
            id.StrategyFingerprint, id.ExecutionPolicyFingerprint, id.ParameterSetFingerprint, id.TemporalPartitionId,
            "No eligible data.", null, null);
        var outcome = ResearchOutcomeArtifactRules.Create(intent, report);
        var package = new ResearchOutcomePackage(new ResearchLaunchPackage(gates, intent), outcome);
        var root = Path.Combine(Path.GetTempPath(), "qf-outcome-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new ResearchOutcomeArtifactFileStore(root);
            store.Save(package);
            Assert.Equal(package, store.Load(outcome.ArtifactFingerprint));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}
