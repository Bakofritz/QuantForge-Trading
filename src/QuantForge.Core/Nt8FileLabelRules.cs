using System.Text.RegularExpressions;

namespace QuantForge.Core;

public enum Nt8FileLabelStatus { MatchingDeclaredLabel, InvalidDeclaration, UnrecognizedFileName, Mismatch }

/// <summary>Filename consistency only. Labels never authenticate the contents or grant admission.</summary>
public static class Nt8FileLabelRules
{
    private static readonly Regex Contract = new(@"\A[A-Z][A-Z0-9]{0,11} (?:0[1-9]|1[0-2])-[0-9]{2}\z",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex FileLabel = new(@"\A(?<contract>[A-Z][A-Z0-9]{0,11} (?:0[1-9]|1[0-2])-[0-9]{2})\.(?<series>Last|Bid|Ask)\.txt\z",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    // An untrusted label suggestion, never proof of contents, interval or timezone.
    public static Nt8MinuteDescriptor? Detect(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > 128) return null;
        var match = FileLabel.Match(fileName);
        if (!match.Success) return null;
        return new(match.Groups["contract"].Value.ToUpperInvariant(),
            Enum.Parse<MarketPriceSeries>(match.Groups["series"].Value, true));
    }

    public static Nt8FileLabelStatus Check(string? fileName, Nt8MinuteDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (descriptor.Instrument is null || descriptor.Instrument.Length > 64 ||
            !Contract.IsMatch(descriptor.Instrument) || !Enum.IsDefined(descriptor.PriceSeries))
            return Nt8FileLabelStatus.InvalidDeclaration;
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > 128)
            return Nt8FileLabelStatus.UnrecognizedFileName;
        var match = FileLabel.Match(fileName);
        if (!match.Success) return Nt8FileLabelStatus.UnrecognizedFileName;
        return string.Equals(match.Groups["contract"].Value, descriptor.Instrument, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(match.Groups["series"].Value, descriptor.PriceSeries.ToString(), StringComparison.OrdinalIgnoreCase)
            ? Nt8FileLabelStatus.MatchingDeclaredLabel : Nt8FileLabelStatus.Mismatch;
    }
}
