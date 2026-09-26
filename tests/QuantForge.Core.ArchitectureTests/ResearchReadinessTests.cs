using QuantForge.Core;
using Xunit;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchReadinessTests
{
    [Fact]
    public void Admitted_dataset_without_coverage_is_not_ready_for_remaining_gates()
    {
        var entry = TestEntry("AAA");
        var result = ResearchReadinessRules.Evaluate(entry, null);
        Assert.True(result.CatalogAdmitted);
        Assert.False(result.AuthoritativeSessionCoverageStored);
        Assert.False(result.ReadyForRemainingResearchGates);
    }

    [Fact]
    public void Matching_authoritative_coverage_advances_only_to_remaining_gates()
    {
        var entry = TestEntry("AAA");
        var report = SessionCoverageRules.Analyze(entry.DatasetFingerprint,
            new[] { new DateTimeOffset(2026, 1, 2, 14, 30, 0, TimeSpan.Zero) },
            new SessionCoveragePolicy("CME-RTH", "v1", true, new[]
            {
                new SessionInterval(new DateTimeOffset(2026, 1, 2, 14, 30, 0, TimeSpan.Zero),
                    new DateTimeOffset(2026, 1, 2, 14, 31, 0, TimeSpan.Zero))
            }));
        var artifact = SessionCoverageArtifactRules.Create(report);
        var result = ResearchReadinessRules.Evaluate(entry, artifact);
        Assert.True(result.ReadyForRemainingResearchGates);
        Assert.Equal(artifact.ArtifactFingerprint, result.SessionCoverageArtifactFingerprint);
        Assert.Contains("strategy admission", result.Limitation, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Coverage_for_different_dataset_fails_closed()
    {
        var entry = TestEntry("AAA");
        var other = SessionCoverageRules.Analyze("BBB",
            new[] { new DateTimeOffset(2026, 1, 2, 14, 30, 0, TimeSpan.Zero) },
            new SessionCoveragePolicy("CME-RTH", "v1", true, new[]
            {
                new SessionInterval(new DateTimeOffset(2026, 1, 2, 14, 30, 0, TimeSpan.Zero),
                    new DateTimeOffset(2026, 1, 2, 14, 31, 0, TimeSpan.Zero))
            }));
        Assert.Throws<InvalidOperationException>(() => ResearchReadinessRules.Evaluate(entry, SessionCoverageArtifactRules.Create(other)));
    }

    private static DatasetCatalogEntry TestEntry(string fingerprint) => new(
        "dataset-1", fingerprint, "MES", "1m",
        new DateTimeOffset(2026, 1, 2, 14, 30, 0, TimeSpan.Zero),
        new DateTimeOffset(2026, 1, 2, 14, 31, 0, TimeSpan.Zero),
        true, new DatasetProvenance("provider", "source", fingerprint));
}
