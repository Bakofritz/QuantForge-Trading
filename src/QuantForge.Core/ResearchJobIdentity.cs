namespace QuantForge.Core;

public readonly record struct ResearchJobIdentity(
    string DatasetFingerprint,
    string StrategyFingerprint,
    string ExecutionPolicyFingerprint,
    string ParameterSetFingerprint,
    string TemporalPartitionId,
    string JobFingerprint);

public static class ResearchJobIdentityRules
{
    public static void RequireComplete(ResearchJobIdentity identity)
    {
        if (string.IsNullOrWhiteSpace(identity.DatasetFingerprint) ||
            string.IsNullOrWhiteSpace(identity.StrategyFingerprint) ||
            string.IsNullOrWhiteSpace(identity.ExecutionPolicyFingerprint) ||
            string.IsNullOrWhiteSpace(identity.ParameterSetFingerprint) ||
            string.IsNullOrWhiteSpace(identity.TemporalPartitionId) ||
            string.IsNullOrWhiteSpace(identity.JobFingerprint))
            throw new InvalidOperationException(
                "Dataset, strategy, execution policy, parameters, temporal partition, and job identity are required.");
    }
}
