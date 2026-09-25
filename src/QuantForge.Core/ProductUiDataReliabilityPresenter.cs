namespace QuantForge.Core;

public enum ProductUiDataReliabilityStatus
{
    Admissible,
    Blocked
}

public sealed record ProductUiDataReliabilityState(
    string DatasetFingerprint,
    decimal ScorePercent,
    bool ComparedToLiveBenchmark,
    int UnresolvedGapCount,
    int ConflictingOverlapCount,
    ProductUiDataReliabilityStatus Status,
    string? Limitation);

public static class ProductUiDataReliabilityPresenter
{
    public static ProductUiDataReliabilityState Create(DataReliabilityAssessment assessment)
    {
        DataReliabilityRules.Validate(assessment);
        var admissible = DataReliabilityRules.IsResearchAdmissible(assessment);

        return new ProductUiDataReliabilityState(
            assessment.DatasetFingerprint,
            assessment.ScorePercent,
            assessment.ComparedToLiveBenchmark,
            assessment.UnresolvedGapCount,
            assessment.ConflictingOverlapCount,
            admissible ? ProductUiDataReliabilityStatus.Admissible : ProductUiDataReliabilityStatus.Blocked,
            assessment.Limitation);
    }
}
