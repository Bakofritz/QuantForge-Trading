using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core;

public sealed record ResearchLaunchIntent(
    string IntentFingerprint,
    string GateBundleFingerprint,
    ResearchJobSpec Job);

public static class ResearchLaunchIntentRules
{
    public static ResearchLaunchIntent Create(ResearchGateBundle gates, ResearchJobSpec job)
    {
        ResearchGateBundleRules.Validate(gates);
        ResearchJobRules.RequireRunnable(job);
        if (!string.Equals(job.Data.DatasetId, gates.Dataset.DatasetId, StringComparison.Ordinal) ||
            !string.Equals(job.Data.DatasetFingerprint, gates.Dataset.DatasetFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Research launch job does not match the admitted dataset gate bundle.");
        if (!string.Equals(job.Strategy.StrategyId, gates.Strategy.Envelope.Manifest.StrategyId, StringComparison.Ordinal) ||
            !string.Equals(job.Strategy.SourceFingerprint, gates.Strategy.Envelope.Manifest.SourceFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Research launch job does not match the admitted strategy gate bundle.");
        if (!string.Equals(job.Identity.DatasetFingerprint, job.Data.DatasetFingerprint, StringComparison.Ordinal) ||
            !string.Equals(job.Identity.StrategyFingerprint, job.Strategy.SourceFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Research launch reproducibility identity does not match the admitted inputs.");
        return new(Fingerprint(gates.BundleFingerprint, job.Identity.JobFingerprint), gates.BundleFingerprint, job);
    }

    public static void Validate(ResearchLaunchIntent intent, ResearchGateBundle gates)
    {
        ArgumentNullException.ThrowIfNull(intent);
        var expected = Create(gates, intent.Job);
        if (!string.Equals(expected.GateBundleFingerprint, intent.GateBundleFingerprint, StringComparison.Ordinal) ||
            !string.Equals(expected.IntentFingerprint, intent.IntentFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Research launch intent fingerprint does not match its admitted evidence and job identity.");
    }

    private static string Fingerprint(string gates, string job) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(gates + "|" + job)));
}
