using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class DatasetCatalogTests
{
    private static DatasetCatalogEntry ValidEntry() => new(
        "MES-NT8-MINUTE",
        "dataset-sha256",
        "MES",
        "1m",
        new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
        new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        true,
        new ProvenanceRecord(
            "artifact-01",
            "fixture://mes-minute",
            "2026-09-26T00:00:00Z",
            "artifact-sha256",
            "original-sha256",
            "dataset-sha256",
            "scrub-01"),
        "dataset-sha256");

    [Fact]
    public void Registered_dataset_produces_admission_from_immutable_identity()
    {
        var catalog = new DatasetCatalog();
        catalog.Register(ValidEntry());

        var admission = catalog.RequireAdmission("MES-NT8-MINUTE");

        Assert.Equal("dataset-sha256", admission.DatasetFingerprint);
        Assert.True(admission.StructuralValidationPassed);
        Assert.True(admission.ProvenanceRecorded);
    }

    [Fact]
    public void Catalog_admission_can_feed_the_existing_research_job_gate()
    {
        var catalog = new DatasetCatalog();
        catalog.Register(ValidEntry());
        var data = catalog.RequireAdmission("MES-NT8-MINUTE");

        var job = new ResearchJobSpec(
            AuthorityDomain.ReadOnlyResearch,
            data,
            new StrategyCapabilityManifest("example", "strategy-sha256", false, false, false, false, false, false, false),
            new ResearchJobIdentity("dataset-sha256", "strategy-sha256", "timing-sha256", "parameters-sha256", "walk-forward-01", "job-sha256"),
            ExecutionTimingPolicy.NextBarOpen,
            false);

        ResearchJobRules.RequireRunnable(job);
    }

    [Fact]
    public void Unknown_dataset_cannot_be_admitted()
    {
        var catalog = new DatasetCatalog();

        Assert.Throws<InvalidOperationException>(() => catalog.RequireAdmission("missing"));
    }

    [Fact]
    public void Conflicting_duplicate_identity_is_rejected()
    {
        var catalog = new DatasetCatalog();
        catalog.Register(ValidEntry());

        var conflicting = ValidEntry() with { DatasetFingerprint = "different-sha256", InspectionFingerprint = "different-sha256" };

        Assert.Throws<InvalidOperationException>(() => catalog.Register(conflicting));
    }

    [Fact]
    public void Provenance_fingerprint_mismatch_is_rejected()
    {
        var invalid = ValidEntry() with
        {
            Provenance = ValidEntry().Provenance with { SanitizedArtifactFingerprint = "other-sha256" }
        };

        var catalog = new DatasetCatalog();

        Assert.Throws<InvalidOperationException>(() => catalog.Register(invalid));
    }

    [Fact]
    public void Failed_structural_validation_cannot_enter_catalog()
    {
        var catalog = new DatasetCatalog();

        Assert.Throws<InvalidOperationException>(() => catalog.Register(ValidEntry() with { StructuralValidationPassed = false }));
    }

    [Fact]
    public void Exact_repeat_registration_is_idempotent()
    {
        var catalog = new DatasetCatalog();
        var entry = ValidEntry();

        catalog.Register(entry);
        catalog.Register(entry);

        Assert.Equal(1, catalog.Count);
    }
}
