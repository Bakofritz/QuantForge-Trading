using QuantForge.Core;
using Xunit;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchOutcomeComparisonTests
{
    [Fact]
    public void Comparison_is_side_by_side_and_does_not_require_performance_state()
    {
        var left = Package("JOB-LEFT", "PARAM-LEFT");
        var right = Package("JOB-RIGHT", "PARAM-RIGHT");
        var comparison = ResearchOutcomeComparisonRules.Create(left, right);
        Assert.Null(comparison.EquityDifference);
        Assert.Equal(ResearchResultStatus.DataBlocked, comparison.LeftStatus);
        Assert.Equal(ResearchResultStatus.DataBlocked, comparison.RightStatus);
        ResearchOutcomeComparisonRules.Validate(comparison, left, right);
    }

    [Fact]
    public void Same_outcome_cannot_be_compared_to_itself()
    {
        var package = Package("JOB-ONE", "PARAM-ONE");
        Assert.Throws<InvalidOperationException>(() => ResearchOutcomeComparisonRules.Create(package, package));
    }

    private static ResearchOutcomePackage Package(string jobFingerprint, string parameterFingerprint)
    {
        var gates = TestFixtures.Gates();
        var baseJob = TestFixtures.Job(gates);
        var identity = baseJob.Identity with { JobFingerprint = jobFingerprint, ParameterSetFingerprint = parameterFingerprint };
        var job = baseJob with { Identity = identity };
        var launch = ResearchLaunchIntentRules.Create(gates, job);
        var report = new ResearchReport(identity.JobFingerprint, ResearchResultStatus.DataBlocked, identity.DatasetFingerprint,
            identity.StrategyFingerprint, identity.ExecutionPolicyFingerprint, identity.ParameterSetFingerprint, identity.TemporalPartitionId,
            "No eligible data.", null, null);
        var outcome = ResearchOutcomeArtifactRules.Create(launch, report);
        return new ResearchOutcomePackage(new ResearchLaunchPackage(gates, launch), outcome);
    }
}
