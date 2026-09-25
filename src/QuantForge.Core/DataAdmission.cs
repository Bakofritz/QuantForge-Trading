namespace QuantForge.Core;

public readonly record struct DataAdmission(
    string DatasetId,
    string DatasetFingerprint,
    string Instrument,
    string Timeframe,
    DateTimeOffset Start,
    DateTimeOffset End,
    bool StructuralValidationPassed,
    bool ProvenanceRecorded);

public static class DataAdmissionRules
{
    public static void RequireAdmitted(DataAdmission admission)
    {
        if (string.IsNullOrWhiteSpace(admission.DatasetId) ||
            string.IsNullOrWhiteSpace(admission.DatasetFingerprint) ||
            string.IsNullOrWhiteSpace(admission.Instrument) ||
            string.IsNullOrWhiteSpace(admission.Timeframe))
            throw new InvalidOperationException("Complete data identity is required.");

        if (admission.End < admission.Start)
            throw new InvalidOperationException("Data range is invalid.");

        if (!admission.StructuralValidationPassed || !admission.ProvenanceRecorded)
            throw new InvalidOperationException(
                "Data admission requires successful structural validation and recorded provenance.");
    }
}
