using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core.ArchitectureTests;

public sealed class StreamingMinuteInspectionTests
{
    private static readonly Nt8MinuteDescriptor Descriptor = new("MES 09-26", MarketPriceSeries.Last);

    [Theory]
    [InlineData(1, "\n", false)]
    [InlineData(2, "\r\n", true)]
    [InlineData(7, "\n", true)]
    public async Task ChunkBoundariesBomAndFinalLinePreserveExactHash(int chunk, string newline, bool bom)
    {
        var bytes = Encoding.UTF8.GetBytes((bom ? "\uFEFF" : "") + Row(0, 26).TrimEnd('\n') + newline + Row(1, 26).TrimEnd('\n'));
        using var stream = new LimitedStream(bytes, chunk);
        var result = await Nt8MinuteInspector.InspectAsync(stream, Descriptor);
        Assert.Equal(MarketDataInspectionStatus.Inspected, result.Status);
        Assert.Equal(2, result.Bars!.Count);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(bytes)), result.SourceFingerprint);
        Assert.True(stream.CanRead);
    }

    [Fact]
    public async Task ExactByteLimitSucceedsAndOneExtraByteRejectsEverything()
    {
        var text = new StringBuilder(Nt8MinuteInspector.MaximumBytes);
        var n = 0;
        while (Nt8MinuteInspector.MaximumBytes - text.Length > 186) text.Append(Row(n++, 161));
        var remaining = Nt8MinuteInspector.MaximumBytes - text.Length;
        if (remaining > 161) { text.Append(Row(n++, 26)); remaining -= 26; }
        text.Append(Row(n++, remaining));
        var bytes = Encoding.ASCII.GetBytes(text.ToString());
        Assert.Equal(Nt8MinuteInspector.MaximumBytes, bytes.Length);
        using (var stream = new LimitedStream(bytes, 65536))
        {
            var result = await Nt8MinuteInspector.InspectAsync(stream, Descriptor);
            Assert.Equal(MarketDataInspectionStatus.Inspected, result.Status);
            Assert.Equal(n, result.Bars!.Count);
            Assert.True(n > 100_000);
            Assert.Equal(Convert.ToHexString(SHA256.HashData(bytes)), result.SourceFingerprint);
        }
        using var oversized = new LimitedStream(bytes.Concat(new byte[] { 10 }).ToArray(), 65536);
        var rejected = await Nt8MinuteInspector.InspectAsync(oversized, Descriptor);
        Assert.Equal("QF-DATA-TOO-LARGE", rejected.DiagnosticCode);
        Assert.Null(rejected.Bars);
        Assert.Null(rejected.SourceFingerprint);
        Assert.Equal(Nt8MinuteInspector.MaximumBytes + 1L, oversized.Position);
    }

    [Fact]
    public async Task LateEncodingFailureCannotPublishEarlierBars()
    {
        var bytes = Encoding.UTF8.GetBytes(Row(0, 26)).Concat(new byte[] { 0xC3, 0x28 }).ToArray();
        using var stream = new LimitedStream(bytes, 1);
        var result = await Nt8MinuteInspector.InspectAsync(stream, Descriptor);
        Assert.Equal("QF-DATA-ENCODING", result.DiagnosticCode);
        Assert.Null(result.Bars);
        Assert.Null(result.SourceFingerprint);
    }

    [Fact]
    public async Task CancellationAfterParsedRowsDiscardsResultAndLeavesCallerStreamOpen()
    {
        using var cancellation = new CancellationTokenSource();
        using var stream = new CancelAfterFirstLine(Encoding.ASCII.GetBytes(Row(0, 26) + Row(1, 26)), cancellation);
        var result = await Nt8MinuteInspector.InspectAsync(stream, Descriptor, cancellation.Token);
        Assert.Equal(MarketDataInspectionStatus.Cancelled, result.Status);
        Assert.Null(result.Bars);
        Assert.Null(result.SourceFingerprint);
        Assert.True(stream.CanRead);
    }

    // Valid all-one OHLCV values padded with leading zeroes, to exercise exact byte budgets.
    private static string Row(int index, int length)
    {
        var fields = new string[5];
        var padding = length - 26;
        for (var i = 0; i < fields.Length; i++)
        {
            var count = Math.Min(27, padding); padding -= count;
            fields[i] = new string('0', count) + "1";
        }
        Assert.Equal(0, padding);
        var row = new DateTime(2020, 1, 1).AddMinutes(index).ToString("yyyyMMdd HHmmss", CultureInfo.InvariantCulture) + ";" + string.Join(';', fields) + "\n";
        Assert.Equal(length, row.Length);
        return row;
    }
    private sealed class LimitedStream(byte[] bytes, int chunk) : MemoryStream(bytes)
    {
        public override bool CanSeek => false;
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            base.ReadAsync(buffer[..Math.Min(buffer.Length, chunk)], cancellationToken);
    }
    private sealed class CancelAfterFirstLine(byte[] bytes, CancellationTokenSource cancellation) : MemoryStream(bytes)
    {
        private int _reads;
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (++_reads == 2) cancellation.Cancel();
            return base.ReadAsync(buffer[..Math.Min(buffer.Length, 26)], CancellationToken.None);
        }
    }
}
