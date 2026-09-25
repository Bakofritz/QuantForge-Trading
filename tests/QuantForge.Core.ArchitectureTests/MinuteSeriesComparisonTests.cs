using System.Text;

namespace QuantForge.Core.ArchitectureTests;

public sealed class MinuteSeriesComparisonTests
{
    private static readonly Nt8MinuteDescriptor Descriptor = new("MES 09-26", MarketPriceSeries.Last);
    private const string First = "20260924 140100;100;102;99;101;10";
    private const string Second = "20260924 140200;101;103;100;102;12";
    private const string Third = "20260924 140300;102;104;101;103;14";

    [Fact]
    public async Task SameBytesAreFlaggedAsNonIndependent()
    {
        var source = await Inspect(First + "\n" + Second);
        var result = MinuteSeriesComparison.Compare(source, source);
        Assert.Equal(MinuteComparisonStatus.Compared, result.Status);
        Assert.True(result.SameSourceBytes);
        Assert.Equal(2, result.MatchingBars);
        Assert.Equal(0, result.ConflictingBars + result.PrimaryOnlyBars + result.ReferenceOnlyBars);
    }

    [Fact]
    public async Task HashLetterCaseDoesNotManufactureSourceIndependence()
    {
        var source = await Inspect(First);
        var reference = source with { SourceFingerprint = source.SourceFingerprint!.ToLowerInvariant() };
        Assert.True(MinuteSeriesComparison.Compare(source, reference).SameSourceBytes);
    }

    [Fact]
    public async Task DifferentEncodingCanAgreeWithoutClaimingIndependentBenchmark()
    {
        var a = await Inspect(First + "\n" + Second);
        var b = await Inspect(First + "\r\n" + Second + "\r\n");
        var result = MinuteSeriesComparison.Compare(a, b);
        Assert.Equal(2, result.MatchingBars);
        Assert.False(result.SameSourceBytes);
        Assert.NotEqual(result.PrimaryFingerprint, result.ReferenceFingerprint);
    }

    [Fact]
    public async Task WholeRangePreservesBothUnmatchedTailsAndIgnoresLocalSequenceOffsets()
    {
        var a = await Inspect(First + "\n" + Second);
        var b = await Inspect(Second + "\n" + Third);
        var result = MinuteSeriesComparison.Compare(a, b);
        Assert.Equal(1, result.MatchingBars);
        Assert.Equal(1, result.PrimaryOnlyBars);
        Assert.Equal(1, result.ReferenceOnlyBars);
        Assert.Equal(0, result.ConflictingBars);
    }

    [Fact]
    public async Task NonOverlappingRangesDoNotAppearToAgree()
    {
        var result = MinuteSeriesComparison.Compare(await Inspect(First), await Inspect(Third));
        Assert.Equal(0, result.MatchingBars);
        Assert.Equal(1, result.PrimaryOnlyBars);
        Assert.Equal(1, result.ReferenceOnlyBars);
    }

    [Theory]
    [InlineData("open")]
    [InlineData("high")]
    [InlineData("low")]
    [InlineData("close")]
    [InlineData("volume")]
    public async Task EveryOhlcvFieldParticipatesInExactComparison(string field)
    {
        var values = First.Split(';');
        values[field switch { "open" => 1, "high" => 2, "low" => 3, "close" => 4, _ => 5 }] =
            field switch { "open" => "100.25", "high" => "103", "low" => "98", "close" => "100", _ => "11" };
        var result = MinuteSeriesComparison.Compare(await Inspect(First), await Inspect(string.Join(';', values)));
        Assert.Equal(1, result.ConflictingBars);
        Assert.Equal(0, result.MatchingBars);
    }

    [Theory]
    [InlineData("contract")]
    [InlineData("series")]
    public async Task DeclarationMismatchNeverProducesAgreementCounts(string mismatch)
    {
        var a = await Inspect(First);
        var b = a with { DeclaredDescriptor = mismatch == "contract" ? new("MNQ 09-26", MarketPriceSeries.Last) : new("MES 09-26", MarketPriceSeries.Bid) };
        var result = MinuteSeriesComparison.Compare(a, b);
        Assert.Equal("QF-COMPARE-IDENTITY", result.DiagnosticCode);
        Assert.Equal(MinuteComparisonStatus.Invalid, result.Status);
        Assert.Null(result.PrimaryFingerprint);
        Assert.Equal(0, result.MatchingBars);
    }

    [Theory]
    [InlineData("status")]
    [InlineData("hash")]
    [InlineData("empty")]
    [InlineData("order")]
    [InlineData("price")]
    [InlineData("sequence")]
    [InlineData("timestamp")]
    public async Task MalformedInspectionObjectsCannotProduceComparisonEvidence(string kind)
    {
        var a = await Inspect(First + "\n" + Second);
        var b = kind switch
        {
            "status" => a with { Status = MarketDataInspectionStatus.Invalid },
            "hash" => a with { SourceFingerprint = "not-a-hash" },
            "empty" => a with { Bars = Array.Empty<MarketEvent>() },
            "order" => a with { Bars = new[] { a.Bars![1] with { Sequence = 0 }, a.Bars![0] with { Sequence = 1 } } },
            "price" => a with { Bars = new[] { a.Bars![0] with { High = 1 } } },
            "sequence" => a with { Bars = new[] { a.Bars![0] with { Sequence = 5 } } },
            _ => a with { Bars = new[] { a.Bars![0] with { Timestamp = a.Bars![0].Timestamp.AddSeconds(1) } } }
        };
        var result = MinuteSeriesComparison.Compare(a, b);
        Assert.Equal(MinuteComparisonStatus.Invalid, result.Status);
        Assert.Null(result.PrimaryFingerprint);
        Assert.Equal(0, result.MatchingBars);
    }

    [Fact]
    public async Task CancelledComparisonReturnsNoPartialCountsAndCanRetry()
    {
        var a = await Inspect(First);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var result = MinuteSeriesComparison.Compare(a, a, cancellation.Token);
        Assert.Equal(MinuteComparisonStatus.Cancelled, result.Status);
        Assert.Equal(0, result.MatchingBars);
        Assert.Null(result.PrimaryFingerprint);
        Assert.Equal(MinuteComparisonStatus.Compared, MinuteSeriesComparison.Compare(a, a).Status);
    }

    private static async Task<Nt8MinuteInspectionResult> Inspect(string text)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        return await Nt8MinuteInspector.InspectAsync(stream, Descriptor);
    }
}
