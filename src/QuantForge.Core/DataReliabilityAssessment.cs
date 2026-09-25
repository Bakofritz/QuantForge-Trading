namespace QuantForge.Core;

public readonly record struct DataReliabilityAssessment(
    string DatasetFingerprint,
    decimal ScorePercent,
    bool ComparedToLiveBenchmark,
    int UnresolvedGapCount,
    int ConflictingOverlapCount,
    string? Limitation);

public static class DataReliabilityRules
{
    public static void Validate(DataReliabilityAssessment assessment)
    {
        if (string.IsNullOrWhiteSpace(assessment.DatasetFingerprint))
            throw new InvalidOperationException("Data reliability requires dataset identity.");

        if (assessment.ScorePercent is < 0m or > 100m)
            throw new InvalidOperationException("Data reliability score must be between 0 and 100 percent.");

        if (assessment.UnresolvedGapCount < 0 || assessment.ConflictingOverlapCount < 0)
            throw new InvalidOperationException("Data reliability issue counts cannot be negative.");

        if (!assessment.ComparedToLiveBenchmark && string.IsNullOrWhiteSpace(assessment.Limitation))
            throw new InvalidOperationException("A reliability assessment without a live benchmark requires an explicit limitation.");

        if ((assessment.UnresolvedGapCount > 0 || assessment.ConflictingOverlapCount > 0) &&
            string.IsNullOrWhiteSpace(assessment.Limitation))
            throw new InvalidOperationException("Unresolved gaps or conflicting overlaps require an explicit limitation.");
    }

    public static bool IsResearchAdmissible(DataReliabilityAssessment assessment)
    {
        Validate(assessment);
        return assessment.ComparedToLiveBenchmark &&
               assessment.UnresolvedGapCount == 0 &&
               assessment.ConflictingOverlapCount == 0;
    }
}
