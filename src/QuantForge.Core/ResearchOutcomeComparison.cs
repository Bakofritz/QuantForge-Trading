using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core;

public sealed record ResearchOutcomeComparison(
    string ComparisonFingerprint,
    string DatasetFingerprint,
    string LeftOutcomeFingerprint,
    string RightOutcomeFingerprint,
    ResearchResultStatus LeftStatus,
    ResearchResultStatus RightStatus,
    decimal? LeftEquity,
    decimal? RightEquity,
    decimal? EquityDifference);

public static class ResearchOutcomeComparisonRules
{
    public static ResearchOutcomeComparison Create(ResearchOutcomePackage left, ResearchOutcomePackage right)
    {
        ValidatePackage(left);
        ValidatePackage(right);

        if (!string.Equals(left.Outcome.Report.DatasetFingerprint, right.Outcome.Report.DatasetFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Research outcome comparison requires the same admitted dataset fingerprint.");
        if (string.Equals(left.Outcome.ArtifactFingerprint, right.Outcome.ArtifactFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Research outcome comparison requires two distinct terminal outcomes.");

        var leftEquity = left.Outcome.Report.Status == ResearchResultStatus.Complete ? left.Outcome.Report.Account?.Equity : null;
        var rightEquity = right.Outcome.Report.Status == ResearchResultStatus.Complete ? right.Outcome.Report.Account?.Equity : null;
        decimal? difference = leftEquity.HasValue && rightEquity.HasValue ? rightEquity.Value - leftEquity.Value : null;
        var fingerprint = Fingerprint(left.Outcome, right.Outcome, difference);

        return new(
            fingerprint,
            left.Outcome.Report.DatasetFingerprint,
            left.Outcome.ArtifactFingerprint,
            right.Outcome.ArtifactFingerprint,
            left.Outcome.Report.Status,
            right.Outcome.Report.Status,
            leftEquity,
            rightEquity,
            difference);
    }

    public static void Validate(ResearchOutcomeComparison comparison, ResearchOutcomePackage left, ResearchOutcomePackage right)
    {
        ArgumentNullException.ThrowIfNull(comparison);
        var expected = Create(left, right);
        if (expected != comparison)
            throw new InvalidOperationException("Research outcome comparison does not match the supplied terminal evidence.");
    }

    private static void ValidatePackage(ResearchOutcomePackage package)
    {
        ArgumentNullException.ThrowIfNull(package);
        ResearchGateBundleRules.Validate(package.Launch.Gates);
        ResearchLaunchIntentRules.Validate(package.Launch.Intent, package.Launch.Gates);
        ResearchOutcomeArtifactRules.Validate(package.Outcome, package.Launch.Intent);
    }

    private static string Fingerprint(ResearchOutcomeArtifact left, ResearchOutcomeArtifact right, decimal? difference)
    {
        var payload = string.Join("|", new[]
        {
            left.ArtifactFingerprint,
            right.ArtifactFingerprint,
            left.Report.Status.ToString(),
            right.Report.Status.ToString(),
            left.Report.Account?.Equity.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            right.Report.Account?.Equity.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            difference?.ToString(CultureInfo.InvariantCulture) ?? string.Empty
        });
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
    }
}
