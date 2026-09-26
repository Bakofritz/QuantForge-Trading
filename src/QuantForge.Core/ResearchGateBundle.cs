using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core;

public sealed record ResearchGateBundle(
    string BundleFingerprint,
    DatasetCatalogEntry Dataset,
    SessionCoverageArtifact SessionCoverage,
    DataReliabilityAssessment Reliability,
    StrategyAdmissionArtifact Strategy);

public static class ResearchGateBundleRules
{
    public static ResearchGateBundle Create(
        DatasetCatalogEntry dataset,
        SessionCoverageArtifact sessionCoverage,
        DataReliabilityAssessment reliability,
        StrategyAdmissionArtifact strategy)
    {
        var catalog = new DatasetCatalog();
        catalog.Register(dataset);
        _ = catalog.RequireAdmission(dataset.DatasetId);
        SessionCoverageArtifactRules.Validate(sessionCoverage);
        DataReliabilityRules.Validate(reliability);
        StrategyAdmissionArtifactRules.Validate(strategy);

        if (!string.Equals(dataset.DatasetFingerprint, sessionCoverage.Report.DatasetFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Session coverage does not belong to the admitted dataset.");
        if (!string.Equals(dataset.DatasetFingerprint, reliability.DatasetFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Reliability evidence does not belong to the admitted dataset.");
        if (!DataReliabilityRules.IsResearchAdmissible(reliability))
            throw new InvalidOperationException("Research gate bundle requires research-admissible reliability evidence.");

        return new(Fingerprint(dataset, sessionCoverage, reliability, strategy), dataset, sessionCoverage, reliability, strategy);
    }

    public static void Validate(ResearchGateBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        var expected = Create(bundle.Dataset, bundle.SessionCoverage, bundle.Reliability, bundle.Strategy);
        if (!string.Equals(expected.BundleFingerprint, bundle.BundleFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Research gate bundle fingerprint does not match its evidence.");
    }

    private static string Fingerprint(
        DatasetCatalogEntry dataset,
        SessionCoverageArtifact sessionCoverage,
        DataReliabilityAssessment reliability,
        StrategyAdmissionArtifact strategy)
    {
        var text = string.Join("|", new[]
        {
            dataset.DatasetId,
            dataset.DatasetFingerprint,
            sessionCoverage.ArtifactFingerprint,
            reliability.ScorePercent.ToString(CultureInfo.InvariantCulture),
            reliability.ComparedToLiveBenchmark.ToString(),
            reliability.UnresolvedGapCount.ToString(CultureInfo.InvariantCulture),
            reliability.ConflictingOverlapCount.ToString(CultureInfo.InvariantCulture),
            reliability.Limitation ?? string.Empty,
            strategy.ArtifactFingerprint
        });
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
    }
}
