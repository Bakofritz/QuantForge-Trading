using QuantForge.Core;
using Xunit;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchOutcomeComparisonFileStoreTests
{
    [Fact]
    public void Comparison_package_round_trips_with_both_outcomes_revalidated()
    {
        var left = Package("JOB-L", "PARAM-L");
        var right = Package("JOB-R", "PARAM-R");
        var comparison = ResearchOutcomeComparisonRules.Create(left, right);
        var package = new ResearchOutcomeComparisonPackage(left, right, comparison);
        var root = Path.Combine(Path.GetTempPath(), "qf-compare-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new ResearchOutcomeComparisonFileStore(root);
            store.Save(package);
            Assert.Equal(package, store.Load(comparison.ComparisonFingerprint));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
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
