using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core;

public sealed record ResearchOutcomeArtifact(
    string ArtifactFingerprint,
    string LaunchIntentFingerprint,
    ResearchReport Report);

public static class ResearchOutcomeArtifactRules
{
    public static ResearchOutcomeArtifact Create(ResearchLaunchIntent launch, ResearchReport report)
    {
        ArgumentNullException.ThrowIfNull(launch);
        ResearchReportRules.Validate(report);
        if (!string.Equals(report.JobId, launch.Job.Identity.JobFingerprint, StringComparison.Ordinal) ||
            !string.Equals(report.DatasetFingerprint, launch.Job.Identity.DatasetFingerprint, StringComparison.Ordinal) ||
            !string.Equals(report.StrategyFingerprint, launch.Job.Identity.StrategyFingerprint, StringComparison.Ordinal) ||
            !string.Equals(report.ExecutionPolicyFingerprint, launch.Job.Identity.ExecutionPolicyFingerprint, StringComparison.Ordinal) ||
            !string.Equals(report.ParameterFingerprint, launch.Job.Identity.ParameterSetFingerprint, StringComparison.Ordinal) ||
            !string.Equals(report.TemporalPartition, launch.Job.Identity.TemporalPartitionId, StringComparison.Ordinal))
            throw new InvalidOperationException("Research outcome report does not match its launch identity.");
        return new(Fingerprint(launch.IntentFingerprint, report), launch.IntentFingerprint, report);
    }

    public static void Validate(ResearchOutcomeArtifact artifact, ResearchLaunchIntent launch)
    {
        ArgumentNullException.ThrowIfNull(artifact);
        if (!string.Equals(artifact.LaunchIntentFingerprint, launch.IntentFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Research outcome does not reference the supplied launch intent.");
        var expected = Create(launch, artifact.Report);
        if (!string.Equals(expected.ArtifactFingerprint, artifact.ArtifactFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Research outcome artifact fingerprint does not match its terminal report.");
    }

    private static string Fingerprint(string launchFingerprint, ResearchReport report)
    {
        var payload = string.Join("|", new[]
        {
            launchFingerprint,
            report.JobId,
            report.Status.ToString(),
            report.DatasetFingerprint,
            report.StrategyFingerprint,
            report.ExecutionPolicyFingerprint,
            report.ParameterFingerprint,
            report.TemporalPartition,
            report.BlockReason ?? string.Empty,
            report.Account?.Equity.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            report.ExecutionTrace?.TraceFingerprint ?? string.Empty
        });
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
    }
}
