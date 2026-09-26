using QuantForge.Core;
using Xunit;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchWorkspaceIndexTests
{
    [Fact]
    public void Workspace_index_is_canonical_and_round_trips()
    {
        var gates = TestFixtures.Gates();
        var launch = ResearchLaunchIntentRules.Create(gates, TestFixtures.Job(gates));
        var id = launch.Job.Identity;
        var report = new ResearchReport(id.JobFingerprint, ResearchResultStatus.DataBlocked, id.DatasetFingerprint,
            id.StrategyFingerprint, id.ExecutionPolicyFingerprint, id.ParameterSetFingerprint, id.TemporalPartitionId,
            "No eligible data.", null, null);
        var outcome = ResearchOutcomeArtifactRules.Create(launch, report);
        var index = ResearchWorkspaceIndexRules.Create(new[]
        {
            ResearchWorkspaceIndexRules.From(outcome),
            ResearchWorkspaceIndexRules.From(gates),
            ResearchWorkspaceIndexRules.From(launch)
        });
        var root = Path.Combine(Path.GetTempPath(), "qf-workspace-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new ResearchWorkspaceIndexFileStore(Path.Combine(root, "workspace.json"));
            store.Save(index);
            Assert.Equal(index, store.Load());
            Assert.Equal("research-gates", index.Entries[0].EntryType);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void Duplicate_reference_is_rejected()
    {
        var entry = ResearchWorkspaceIndexRules.From(TestFixtures.Gates());
        Assert.Throws<InvalidOperationException>(() => ResearchWorkspaceIndexRules.Create(new[] { entry, entry }));
    }
}
