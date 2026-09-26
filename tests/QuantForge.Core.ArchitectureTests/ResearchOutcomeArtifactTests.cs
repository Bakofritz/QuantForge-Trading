using QuantForge.Core;
using Xunit;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchOutcomeArtifactTests
{
    [Fact]
    public void Blocked_terminal_report_is_bound_to_launch_identity()
    {
        var gates = TestFixtures.Gates();
        var launch = ResearchLaunchIntentRules.Create(gates, TestFixtures.Job(gates));
        var id = launch.Job.Identity;
        var report = new ResearchReport(id.JobFingerprint, ResearchResultStatus.DataBlocked, id.DatasetFingerprint,
            id.StrategyFingerprint, id.ExecutionPolicyFingerprint, id.ParameterSetFingerprint, id.TemporalPartitionId,
            "No eligible data.", null, null);
        var artifact = ResearchOutcomeArtifactRules.Create(launch, report);
        ResearchOutcomeArtifactRules.Validate(artifact, launch);
    }

    [Fact]
    public void Mismatched_job_is_rejected()
    {
        var gates = TestFixtures.Gates();
        var launch = ResearchLaunchIntentRules.Create(gates, TestFixtures.Job(gates));
        var id = launch.Job.Identity;
        var report = new ResearchReport("OTHER", ResearchResultStatus.DataBlocked, id.DatasetFingerprint,
            id.StrategyFingerprint, id.ExecutionPolicyFingerprint, id.ParameterSetFingerprint, id.TemporalPartitionId,
            "No eligible data.", null, null);
        Assert.Throws<InvalidOperationException>(() => ResearchOutcomeArtifactRules.Create(launch, report));
    }
}
