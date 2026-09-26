using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class MarketAdmissionReadinessTests
{
    private static MarketBatchFile ValidFile(string hash = "dataset-sha")
    {
        var rows = new[]
        {
            new MarketTextRow(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), 1, 2, 0, 1, 10),
            new MarketTextRow(new DateTimeOffset(2026, 1, 1, 0, 1, 0, TimeSpan.Zero), 1, 3, 0, 2, 12)
        };
        var data = new MarketTextResult("QF-BATCH-INSPECTED", MarketTextKind.Minute, rows, hash, 10, 1);
        return new MarketBatchFile(1, "MES 09-26.Minute.Last.txt", null, MarketFileState.Inspected,
            "QF-BATCH-INSPECTED", new Nt8MinuteDescriptor("MES 09-26", MarketPriceSeries.Last),
            "Unverified filename label", data);
    }

    private static ProvenanceRecord Provenance(string hash) => new(
        "artifact-01", "fixture://market", "2026-09-26T00:00:00Z", "source-sha", "original-sha", hash, "scrub-01");

    [Fact]
    public void InspectionAloneNeverClaimsAdmissionReadiness()
    {
        var readiness = MarketAdmissionReadinessRules.Assess(ValidFile());
        Assert.Equal(MarketAdmissionReadinessStatus.NeedsEvidence, readiness.Status);
        Assert.Contains(MarketAdmissionRequirement.ExternalDatasetId, readiness.Requirements);
        Assert.Contains(MarketAdmissionRequirement.ExternalInstrumentIdentity, readiness.Requirements);
        Assert.Contains(MarketAdmissionRequirement.ExternalTimeframeIdentity, readiness.Requirements);
        Assert.Contains(MarketAdmissionRequirement.ImmutableProvenance, readiness.Requirements);
    }

    [Fact]
    public void ExactIndependentEvidenceCanReachReadyWithoutMutatingCatalog()
    {
        var readiness = MarketAdmissionReadinessRules.Assess(
            ValidFile(), "MES-DATASET", "MES 09-26", "1m", Provenance("dataset-sha"));
        Assert.Equal(MarketAdmissionReadinessStatus.Ready, readiness.Status);
        Assert.Empty(readiness.Requirements);
        Assert.True(readiness.CanCreateAdmissionRequest);
    }

    [Fact]
    public void FingerprintOrIdentityConflictBlocksReadiness()
    {
        var fingerprint = MarketAdmissionReadinessRules.Assess(
            ValidFile(), "MES-DATASET", "MES 09-26", "1m", Provenance("different"));
        Assert.Equal(MarketAdmissionReadinessStatus.Blocked, fingerprint.Status);
        Assert.Contains(MarketAdmissionRequirement.FingerprintAgreement, fingerprint.Requirements);

        var identity = MarketAdmissionReadinessRules.Assess(
            ValidFile(), "MES-DATASET", "MNQ 09-26", "1m", Provenance("dataset-sha"));
        Assert.Equal(MarketAdmissionReadinessStatus.Blocked, identity.Status);
        Assert.Contains(MarketAdmissionRequirement.ResolveDescriptiveLabelConflict, identity.Requirements);
    }

    [Fact]
    public void DuplicateUnresolvedAndConflictStatesRemainBlocked()
    {
        foreach (var state in new[] { MarketFileState.Duplicate, MarketFileState.UnresolvedIdentity, MarketFileState.IdentityConflict })
        {
            var readiness = MarketAdmissionReadinessRules.Assess(
                ValidFile() with { State = state }, "MES-DATASET", "MES 09-26", "1m", Provenance("dataset-sha"));
            Assert.Equal(MarketAdmissionReadinessStatus.Blocked, readiness.Status);
            Assert.Contains(MarketAdmissionRequirement.AdmissionEligibleCandidate, readiness.Requirements);
        }
    }
}
