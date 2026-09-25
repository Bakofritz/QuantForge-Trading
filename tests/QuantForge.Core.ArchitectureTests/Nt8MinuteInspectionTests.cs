using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core.ArchitectureTests;

public sealed class Nt8MinuteInspectionTests
{
    private const string First = "20260924 140100;100.25;102;99;101;10";
    private const string Second = "20260924 140200;101;103;100;102;12";
    private static readonly Nt8MinuteDescriptor Descriptor = new("MES 09-26", MarketPriceSeries.Last);

    [Theory]
    [InlineData(false, "\n")]
    [InlineData(true, "\r\n")]
    public async Task InspectsExactBytesUtcEndStampsAndReadOnlyBars(bool bom, string newline)
    {
        var payload = Encoding.UTF8.GetBytes(First + newline + Second + newline);
        var bytes = bom ? new byte[] { 0xEF, 0xBB, 0xBF }.Concat(payload).ToArray() : payload;
        using var stream = new ChunkedStream(bytes);
        var result = await Nt8MinuteInspector.InspectAsync(stream, Descriptor);
        Assert.Equal(MarketDataInspectionStatus.Inspected, result.Status);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(bytes)), result.SourceFingerprint);
        Assert.Equal(Descriptor, result.DeclaredDescriptor);
        var bars = Assert.IsAssignableFrom<IReadOnlyList<MarketEvent>>(result.Bars);
        Assert.Equal(2, bars.Count);
        Assert.Equal(new DateTimeOffset(2026, 9, 24, 14, 1, 0, TimeSpan.Zero), bars[0].Timestamp);
        Assert.Equal(100.25m, bars[0].Open);
        Assert.Equal(1, bars[1].Sequence);
        Assert.Throws<NotSupportedException>(() => ((IList<MarketEvent>)bars)[0] = default);
        Assert.Equal(0, result.NonContiguousIntervals);
        Assert.True(stream.CanRead);
    }

    [Theory]
    [InlineData("")]
    [InlineData("\n")]
    [InlineData("20260924;100;102;99;101;10")]
    [InlineData("20260924 140101;100;102;99;101;10")]
    [InlineData("20260230 140100;100;102;99;101;10")]
    [InlineData("20260924 140100;100;102;99;101;10;extra")]
    [InlineData("20260924 140100;100;99;99;101;10")]
    [InlineData("20260924 140100;100;102;101;101;10")]
    [InlineData("20260924 140100;-100;102;99;101;10")]
    [InlineData("20260924 140100;1e2;102;99;101;10")]
    [InlineData("20260924 140100;100,25;102;99;101;10")]
    [InlineData("20260924 140100;100;102;99;101;10.5")]
    [InlineData("20260924 140100;100;102;99;101; 10")]
    [InlineData("20260924 140100;100;102;99;101;0.00000000000000000000000000001")]
    public async Task InvalidRowsNeverReturnPartialDatasetOrFingerprint(string text)
    {
        var result = await Inspect(text);
        Assert.Equal(MarketDataInspectionStatus.Invalid, result.Status);
        Assert.Null(result.Bars);
        Assert.Null(result.SourceFingerprint);
        Assert.Null(result.DeclaredDescriptor);
    }

    [Theory]
    [InlineData("duplicate")]
    [InlineData("reverse")]
    [InlineData("blank")]
    [InlineData("corrupt")]
    public async Task BadTailDiscardsAllPreviouslyParsedBars(string kind)
    {
        var text = kind switch
        {
            "duplicate" => First + "\n" + First,
            "reverse" => Second + "\n" + First,
            "blank" => First + "\n\n" + Second,
            _ => First + "\ninvalid"
        };
        var result = await Inspect(text);
        Assert.Equal(MarketDataInspectionStatus.Invalid, result.Status);
        Assert.Equal(2, result.ErrorLine);
        Assert.Null(result.Bars);
        Assert.Null(result.SourceFingerprint);
    }

    [Fact]
    public async Task SessionBreakIsReportedWithoutInventingBarsOrCoverage()
    {
        var result = await Inspect(First + "\n" + Second.Replace("140200", "150200", StringComparison.Ordinal));
        Assert.Equal(MarketDataInspectionStatus.Inspected, result.Status);
        Assert.Equal(1, result.NonContiguousIntervals);
        Assert.Equal(2, result.Bars!.Count);
    }

    [Fact]
    public async Task InvariantNumbersIgnoreDeviceLocale()
    {
        var before = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            Assert.Equal(100.25m, (await Inspect(First)).Bars![0].Open);
        }
        finally { CultureInfo.CurrentCulture = before; }
    }

    [Fact]
    public async Task DescriptorDoesNotComeFromFilenameOrDefaultPriceSeries()
    {
        foreach (var descriptor in new[] { new Nt8MinuteDescriptor("", MarketPriceSeries.Last),
            new("MES/../MNQ", MarketPriceSeries.Last), new("MES", (MarketPriceSeries)999),
            new(" MES", MarketPriceSeries.Last), new(new string('M', 65), MarketPriceSeries.Bid) })
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(First));
            Assert.Equal("QF-DATA-DESCRIPTOR", (await Nt8MinuteInspector.InspectAsync(stream, descriptor)).DiagnosticCode);
            Assert.Equal(0, stream.Position);
        }
    }

    [Fact]
    public async Task ResourceLimitsStopWithoutPublishingPartialData()
    {
        using var oversized = new MemoryStream(new byte[Nt8MinuteInspector.MaximumBytes + 10]);
        var result = await Nt8MinuteInspector.InspectAsync(oversized, Descriptor);
        Assert.Equal("QF-DATA-TOO-LARGE", result.DiagnosticCode);
        Assert.Equal(Nt8MinuteInspector.MaximumBytes + 1L, oversized.Position);
        Assert.Null(result.Bars);
        Assert.Equal("QF-DATA-LINE", (await Inspect(new string('1', 257))).DiagnosticCode);
        var builder = new StringBuilder();
        var time = new DateTime(2026, 1, 1);
        for (var i = 0; i <= Nt8MinuteInspector.MaximumBars; i++)
            builder.Append(time.AddMinutes(i).ToString("yyyyMMdd HHmmss", CultureInfo.InvariantCulture)).Append(";1;1;1;1;1\n");
        Assert.Equal("QF-DATA-TOO-MANY-BARS", (await Inspect(builder.ToString())).DiagnosticCode);
    }

    [Fact]
    public async Task CancelledUnavailableAndBadEncodingCanRecover()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(First));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var cancelled = await Nt8MinuteInspector.InspectAsync(stream, Descriptor, cancellation.Token);
        Assert.Equal(MarketDataInspectionStatus.Cancelled, cancelled.Status);
        Assert.Null(cancelled.Bars);
        Assert.Equal(0, stream.Position);
        using var corrupt = new MemoryStream(new byte[] { 0xC3, 0x28 });
        Assert.Equal("QF-DATA-ENCODING", (await Nt8MinuteInspector.InspectAsync(corrupt, Descriptor)).DiagnosticCode);
        using var failed = new BrokenStream();
        Assert.Equal(MarketDataInspectionStatus.Unavailable, (await Nt8MinuteInspector.InspectAsync(failed, Descriptor)).Status);
        Assert.Equal(MarketDataInspectionStatus.Inspected, (await Nt8MinuteInspector.InspectAsync(stream, Descriptor)).Status);
    }

    [Fact]
    public async Task CancellationDuringReadCannotPublishCompletedBytes()
    {
        using var cancellation = new CancellationTokenSource();
        using var stream = new CancellingStream(Encoding.UTF8.GetBytes(First), cancellation);
        var result = await Nt8MinuteInspector.InspectAsync(stream, Descriptor, cancellation.Token);
        Assert.Equal(MarketDataInspectionStatus.Cancelled, result.Status);
        Assert.Null(result.Bars);
        Assert.Null(result.SourceFingerprint);
        Assert.True(stream.CanRead);
    }

    private sealed class CancellingStream(byte[] bytes, CancellationTokenSource cancellation) : MemoryStream(bytes)
    {
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            // Simulate a provider returning bytes even as cancellation is requested.
            var count = await base.ReadAsync(buffer, CancellationToken.None);
            cancellation.Cancel();
            return count;
        }
    }

    private static async Task<Nt8MinuteInspectionResult> Inspect(string text)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        return await Nt8MinuteInspector.InspectAsync(stream, Descriptor);
    }
    private sealed class ChunkedStream(byte[] bytes) : MemoryStream(bytes)
    {
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            base.ReadAsync(buffer[..Math.Min(buffer.Length, 7)], cancellationToken);
    }
    private sealed class BrokenStream : MemoryStream
    {
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            throw new IOException("Do not expose private provider details.");
    }
}
