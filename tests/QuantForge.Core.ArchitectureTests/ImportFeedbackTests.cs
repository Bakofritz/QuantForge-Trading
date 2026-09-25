namespace QuantForge.Core.ArchitectureTests;

public sealed class ImportFeedbackTests
{
    [Theory]
    [InlineData("mes 09-26.last.TXT", "MES 09-26", MarketPriceSeries.Last)]
    [InlineData("MNQ 12-25.Bid.txt", "MNQ 12-25", MarketPriceSeries.Bid)]
    [InlineData("MES 03-26.Ask.txt", "MES 03-26", MarketPriceSeries.Ask)]
    public void DetectsLabelWithoutAcceptingDifferentReference(string file, string contract, MarketPriceSeries series)
    {
        var descriptor = Assert.IsType<Nt8MinuteDescriptor>(Nt8FileLabelRules.Detect(file));
        Assert.Equal(contract, descriptor.Instrument);
        Assert.Equal(series, descriptor.PriceSeries);
        Assert.Equal(Nt8FileLabelStatus.Mismatch, Nt8FileLabelRules.Check("ES 06-20.Last.txt", descriptor));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("../MES 09-26.Last.txt")]
    [InlineData("MES 13-26.Last.txt")]
    [InlineData("MES 09-26.Last.txt.exe")]
    [InlineData("unknown.txt")]
    public void DoesNotGuessUnrecognizedLabels(string? file) => Assert.Null(Nt8FileLabelRules.Detect(file));

    [Theory]
    [InlineData("QF-DATA-TOO-LARGE", DiagnosticOutcome.FileTooLarge)]
    [InlineData("QF-MANIFEST-TOO-LARGE", DiagnosticOutcome.FileTooLarge)]
    [InlineData("QF-DATA-TOO-MANY-BARS", DiagnosticOutcome.TooManyBars)]
    [InlineData("QF-DATA-FORMAT", DiagnosticOutcome.InvalidFormat)]
    [InlineData("QF-DATA-ORDER", DiagnosticOutcome.InvalidOrder)]
    [InlineData("/private/user-file.txt", DiagnosticOutcome.Invalid)]
    public void DiagnosticReasonsNeverEchoInput(string code, DiagnosticOutcome reason)
    {
        Assert.Equal(reason, ImportInspectionFeedback.Classify(code));
        Assert.DoesNotContain(code, ImportInspectionFeedback.Describe(code));
    }
}
