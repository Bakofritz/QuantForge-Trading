namespace QuantForge.Core.ArchitectureTests;

public sealed class Nt8FileLabelRulesTests
{
    [Theory]
    [InlineData("MES 09-26.Last.txt", "MES 09-26", MarketPriceSeries.Last, Nt8FileLabelStatus.MatchingDeclaredLabel)]
    [InlineData("mes 09-26.last.TXT", "MES 09-26", MarketPriceSeries.Last, Nt8FileLabelStatus.MatchingDeclaredLabel)]
    [InlineData("MES 09-26.Last.txt", "MNQ 09-26", MarketPriceSeries.Last, Nt8FileLabelStatus.Mismatch)]
    [InlineData("MES 09-26.Last.txt", "MES 12-26", MarketPriceSeries.Last, Nt8FileLabelStatus.Mismatch)]
    [InlineData("MES 09-26.Bid.txt", "MES 09-26", MarketPriceSeries.Last, Nt8FileLabelStatus.Mismatch)]
    [InlineData("MES 09-26.Ask.txt", "MES 09-26", MarketPriceSeries.Ask, Nt8FileLabelStatus.MatchingDeclaredLabel)]
    [InlineData("MES 09-26.Last.txt", "anything", MarketPriceSeries.Last, Nt8FileLabelStatus.InvalidDeclaration)]
    [InlineData("MES 09-26.Last.txt", "MES 13-26", MarketPriceSeries.Last, Nt8FileLabelStatus.InvalidDeclaration)]
    [InlineData("MES 09-26.Last.txt", " MES 09-26", MarketPriceSeries.Last, Nt8FileLabelStatus.InvalidDeclaration)]
    [InlineData("MES 09-26.Last.txt", "MES 09-26", (MarketPriceSeries)999, Nt8FileLabelStatus.InvalidDeclaration)]
    [InlineData("renamed.txt", "MES 09-26", MarketPriceSeries.Last, Nt8FileLabelStatus.UnrecognizedFileName)]
    [InlineData("../MES 09-26.Last.txt", "MES 09-26", MarketPriceSeries.Last, Nt8FileLabelStatus.UnrecognizedFileName)]
    [InlineData("MES 09-26.Last.txt.exe", "MES 09-26", MarketPriceSeries.Last, Nt8FileLabelStatus.UnrecognizedFileName)]
    public void RejectsMismatchWithoutAuthenticatingContents(string file, string contract,
        MarketPriceSeries series, Nt8FileLabelStatus expected)
    {
        Assert.Equal(expected, Nt8FileLabelRules.Check(file, new(contract, series)));
    }
}
