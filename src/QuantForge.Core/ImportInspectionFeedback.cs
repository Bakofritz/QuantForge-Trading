namespace QuantForge.Core;

/// <summary>Allowlisted reasons only: never copy input text or exception details into diagnostics.</summary>
public static class ImportInspectionFeedback
{
    public static DiagnosticOutcome Classify(string? code) => code switch
    {
        "QF-DATA-TOO-LARGE" or "QF-MANIFEST-TOO-LARGE" => DiagnosticOutcome.FileTooLarge,
        "QF-DATA-TOO-MANY-BARS" => DiagnosticOutcome.TooManyBars,
        "QF-DATA-FORMAT" or "QF-DATA-LINE" or "QF-MANIFEST-INVALID" => DiagnosticOutcome.InvalidFormat,
        "QF-DATA-NUMBER" or "QF-DATA-VOLUME" or "QF-DATA-OHLCV" => DiagnosticOutcome.InvalidValues,
        "QF-DATA-ORDER" => DiagnosticOutcome.InvalidOrder,
        "QF-DATA-EMPTY" => DiagnosticOutcome.EmptyFile,
        "QF-DATA-ENCODING" => DiagnosticOutcome.InvalidEncoding,
        "QF-DATA-INSPECTED" or "QF-MANIFEST-INSPECTED" => DiagnosticOutcome.Completed,
        "QF-DATA-CANCELLED" or "QF-MANIFEST-CANCELLED" => DiagnosticOutcome.Cancelled,
        "QF-DATA-UNAVAILABLE" or "QF-MANIFEST-UNAVAILABLE" => DiagnosticOutcome.Unavailable,
        _ => DiagnosticOutcome.Invalid
    };

    public static string Describe(string? code) => code switch
    {
        "QF-MANIFEST-TOO-LARGE" => "JSON metadata exceeds 64 KiB. Market-data TXT belongs in Choose market-data TXT",
        "QF-MANIFEST-INVALID" => "This is not a supported QuantForge JSON manifest. Market-data TXT belongs in Choose market-data TXT",
        "QF-DATA-TOO-LARGE" => "Market-data file exceeds the current 8 MiB limit. No partial file was accepted",
        "QF-DATA-TOO-MANY-BARS" => "Market-data file exceeds the current 100,000-bar limit. No partial file was accepted",
        "QF-DATA-FORMAT" => "Expected NT8 UTC one-minute OHLCV text. Daily and tick formats are not supported yet",
        "QF-DATA-ORDER" => "Timestamps repeat or run backwards. Data was not reordered or deduplicated",
        "QF-DATA-EMPTY" => "The selected file contains no bars",
        "QF-DATA-ENCODING" => "The selected file is not valid UTF-8 text",
        _ => "Inspection could not accept this file; check the reported diagnostic code"
    };
}
