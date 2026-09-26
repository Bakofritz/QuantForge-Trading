using System.Text.Json;
using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class SessionCoverageArtifactTests
{
    [Fact]
    public void CompleteAuthoritativeCoverage_IsFingerprintBoundAndRecoverable()
    {
        var report = Report();
        var artifact = SessionCoverageArtifactRules.Create(report);
        var root = Path.Combine(Path.GetTempPath(), "qf-session-artifact-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new SessionCoverageArtifactFileStore(root);
            store.Save(artifact);
            var recovered = store.Load(artifact.ArtifactFingerprint);
            Assert.Equal(artifact, recovered);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void IncompleteCoverage_CannotBecomePersistentAuthorityEvidence()
    {
        var report = Report() with
        {
            MissingMinuteCount = 1,
            MissingMinutes = new[] { new DateTimeOffset(2026, 9, 1, 0, 1, 0, TimeSpan.Zero) }
        };
        Assert.Throws<InvalidOperationException>(() => SessionCoverageArtifactRules.Create(report));
    }

    [Fact]
    public void TamperedStoredReport_FailsFingerprintValidation()
    {
        var artifact = SessionCoverageArtifactRules.Create(Report());
        var root = Path.Combine(Path.GetTempPath(), "qf-session-artifact-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new SessionCoverageArtifactFileStore(root);
            var path = store.Save(artifact);
            var tampered = artifact with { Report = artifact.Report with { ObservedInSessionMinuteCount = 999 } };
            File.WriteAllText(path, JsonSerializer.Serialize(tampered, new JsonSerializerOptions { WriteIndented = true }));
            Assert.Throws<InvalidOperationException>(() => store.Load(artifact.ArtifactFingerprint));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    private static SessionCoverageReport Report() => new(
        new string('A', 64), new string('B', 64), 2, 2, 0, 0,
        Array.Empty<DateTimeOffset>(), Array.Empty<DateTimeOffset>(), true,
        "Explicit UTC session policy supplied; no exchange calendar was inferred.");
}
