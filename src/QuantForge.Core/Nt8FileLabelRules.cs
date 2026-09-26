using System.Text.RegularExpressions;

namespace QuantForge.Core;

public enum Nt8FileLabelStatus { MatchingDeclaredLabel, InvalidDeclaration, UnrecognizedFileName, Mismatch }

/// <summary>Filename consistency only. Labels never authenticate the contents or grant admission.</summary>
public static class Nt8FileLabelRules
{
    private static readonly Regex Contract = new(@"\A[A-Z][A-Z0-9]{0,11} (?:0[1-9]|1[0-2])-[0-9]{2}\z",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    // Flexible filename labels remain unverified; contents determine the batch format.
    public static Nt8MinuteDescriptor? Detect(string? fileName) =>
        fileName?.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) == true
            ? MarketBatchInspection.DetectLabel(fileName) : null;

    public static Nt8FileLabelStatus Check(string? fileName, Nt8MinuteDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (descriptor.Instrument is null || descriptor.Instrument.Length > 64 ||
            !Contract.IsMatch(descriptor.Instrument) || !Enum.IsDefined(descriptor.PriceSeries))
            return Nt8FileLabelStatus.InvalidDeclaration;
        var label = Detect(fileName);
        if (label is null) return Nt8FileLabelStatus.UnrecognizedFileName;
        return string.Equals(label.Instrument, descriptor.Instrument, StringComparison.OrdinalIgnoreCase) &&
            label.PriceSeries == descriptor.PriceSeries
            ? Nt8FileLabelStatus.MatchingDeclaredLabel : Nt8FileLabelStatus.Mismatch;
    }
}
