using System.IO.Compression;
using System.Text;

namespace QuantForge.Core.ArchitectureTests;

public sealed class MarketBatchTests
{
    private const string Minute = "20260925 000100;100;102;100;102;3\n";
    private const string Day = "20260925;100;102;100;102;3\n";
    private const string Tick = "20260925 000015;100;1\n20260925 000045 1234567;102;2\n";
    private static MarketBatchSource Source(string name, string text) => new(name, () => Task.FromResult<Stream>(new MemoryStream(Encoding.UTF8.GetBytes(text))));
    private static MarketBatchSource Zip(params (string Name, string Text)[] members)
    {
        using var memory = new MemoryStream();
        using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, true))
            foreach (var member in members)
            { using var writer = new StreamWriter(archive.CreateEntry(member.Name).Open(), new UTF8Encoding(false)); writer.Write(member.Text); }
        var bytes = memory.ToArray();
        return new("market.zip", () => Task.FromResult<Stream>(new MemoryStream(bytes)));
    }

    [Theory]
    [InlineData("MES 09-26.Minute.Last.txt", "MES 09-26", MarketPriceSeries.Last)]
    [InlineData("minute_MNQ_12-26_bid.txt", "MNQ 12-26", MarketPriceSeries.Bid)]
    [InlineData("MES 09-26 Ask day.txt", "MES 09-26", MarketPriceSeries.Ask)]
    public void FlexibleAnnotationsDoNotRequireRenaming(string name, string contract, MarketPriceSeries series)
    { Assert.Equal(new Nt8MinuteDescriptor(contract, series), Nt8FileLabelRules.Detect(name)); }

    [Theory]
    [InlineData("anything.txt")]
    [InlineData("MES 09-26 MNQ 09-26.Last.txt")]
    [InlineData("MES 09-26.Last.Bid.txt")]
    [InlineData("../MES 09-26.Last.txt")]
    public void AmbiguousNamesAreNotGuessed(string name) => Assert.Null(MarketBatchInspection.DetectLabel(name));

    [Fact]
    public async Task MixedZipAndLooseFilesRemainSeparateAndDetectFormats()
    {
        var result = await MarketBatchInspection.InspectAsync(new[] {
            Zip(("minute/MES 09-26.Minute.Last.txt", Minute), ("day/MES 09-26.Day.Last.txt", Day),
                ("tick/MNQ 09-26.Tick.Last.txt", Tick)), Source("unknown.txt", Minute.Replace("102", "103")) });
        Assert.True(result.Completed);
        Assert.Equal(4, result.Files.Count);
        Assert.Equal(new[] { MarketTextKind.Minute, MarketTextKind.Day, MarketTextKind.Tick, MarketTextKind.Minute }, result.Files.Select(x => x.Data!.Kind));
        Assert.Equal("MNQ 09-26", result.Files[2].Label!.Instrument);
        Assert.Equal(MarketFileState.UnresolvedIdentity, result.Files[3].State);
        Assert.All(result.Files.Take(3), f => Assert.Equal(64, f.ArchiveHash!.Length));
        Assert.Equal(0, MarketCrossValidation.Compare(result).Pairs.Count(x => x.Left == 3 || x.Right == 3));
    }

    [Fact]
    public async Task ExactByteAssociationNeverResolvesConflictingNamedSources()
    {
        var r = await MarketBatchInspection.InspectAsync(new[] { Source("unknown.txt", Minute), Source("MES 09-26.Last.txt", Minute) });
        Assert.Equal("MES 09-26", r.Files[0].Label!.Instrument);
        Assert.Contains("unverified", r.Files[0].LabelBasis);
        Assert.Equal(MarketFileState.Duplicate, r.Files[1].State);
        var conflicting = await MarketBatchInspection.InspectAsync(new[] { Source("MES 09-26.Last.txt", Minute), Source("MNQ 09-26.Last.txt", Minute), Source("unknown.txt", Minute) });
        Assert.All(conflicting.Files, f => Assert.Equal(MarketFileState.IdentityConflict, f.State));
        Assert.Empty(MarketCrossValidation.Compare(conflicting).Pairs);
    }

    [Fact]
    public async Task FailedFileHasNoPartialRowsWhileValidFileHasExplicitIndependentStatus()
    {
        var r = await MarketBatchInspection.InspectAsync(new[] { Source("MES 09-26.Last.txt", Minute), Source("MES 09-26.Minute.Last.txt", Minute + "broken\n") });
        Assert.True(r.Completed);
        Assert.Equal(MarketFileState.Inspected, r.Files[0].State);
        Assert.Equal(MarketFileState.Rejected, r.Files[1].State);
        Assert.Null(r.Files[1].Data!.Rows);
        Assert.Null(r.Files[1].Data!.Hash);
    }

    [Theory]
    [InlineData("../MES 09-26.Last.txt")]
    [InlineData("/MES 09-26.Last.txt")]
    [InlineData("C:/MES 09-26.Last.txt")]
    [InlineData("a\\MES 09-26.Last.txt")]
    public async Task UnsafeArchivePathsInvalidateWholeBatch(string path)
    {
        var r = await MarketBatchInspection.InspectAsync(new[] { Source("MES 09-26.Last.txt", Minute), Zip((path, Minute)) });
        Assert.Equal("QF-BATCH-UNSAFE-ARCHIVE", r.Code); Assert.Empty(r.Files);
    }

    [Fact]
    public async Task DuplicateArchiveNamesAndNestedArchivesAreNotSilentlyAccepted()
    {
        var r = await MarketBatchInspection.InspectAsync(new[] { Zip(("same.txt", Minute), ("SAME.txt", Day)) });
        Assert.Equal("QF-BATCH-UNSAFE-ARCHIVE", r.Code); Assert.Empty(r.Files);
        var nested = await MarketBatchInspection.InspectAsync(new[] { Zip(("nested.zip", "not extracted")) });
        Assert.Equal(MarketFileState.Rejected, Assert.Single(nested.Files).State);
    }

    [Fact]
    public async Task CancellationDropsAllBatchResults()
    {
        using var c = new CancellationTokenSource();
        var second = new MarketBatchSource("MNQ 09-26.Last.txt", () => { c.Cancel(); return Task.FromResult<Stream>(new MemoryStream(Encoding.UTF8.GetBytes(Minute))); });
        var r = await MarketBatchInspection.InspectAsync(new[] { Source("MES 09-26.Last.txt", Minute), second }, c.Token);
        Assert.Equal("QF-BATCH-CANCELLED", r.Code); Assert.Empty(r.Files);
    }

    [Fact]
    public async Task CrossValidationKeepsDailyBoundaryPolicyExplicit()
    {
        var r = await MarketBatchInspection.InspectAsync(new[] { Source("MES 09-26.Minute.Last.txt", Minute), Source("MES 09-26.Tick.Last.txt", Tick), Source("MES 09-26.Day.Last.txt", Day) });
        var safe = MarketCrossValidation.Compare(r);
        Assert.Equal(1, safe.Pairs.Single(p => p.Left == 1 && p.Right == 2).Matching);
        Assert.Equal(2, safe.Pairs.Count(p => p.Code == "QF-CROSS-SESSION-DEFINITION-REQUIRED"));
        var exploratory = MarketCrossValidation.Compare(r, true);
        Assert.All(exploratory.Pairs, p => Assert.Equal(1, p.Matching));
        Assert.Contains("NOT exchange", exploratory.Pairs.Single(p => p.Left == 1 && p.Right == 3).Basis);
    }

    [Fact]
    public async Task TickBoundaryAndEqualTimestampOrderArePreserved()
    {
        var ticks = "20260925 000000;100;1\n20260925 000000;102;2\n20260925 000100;104;1\n";
        var r = await MarketBatchInspection.InspectAsync(new[] { Source("MES 09-26.Tick.Last.txt", ticks), Source("MES 09-26.Minute.Last.txt", Minute) });
        Assert.Equal(3, r.Files[0].Data!.Rows!.Count);
        var pair = Assert.Single(MarketCrossValidation.Compare(r).Pairs);
        Assert.Equal(1, pair.Matching); Assert.Equal(1, pair.LeftOnly);
    }

    [Fact]
    public async Task ReplayQuotesAreRetainedAndCannotBeLabelledBidSeries()
    {
        const string replay = "20260925 000015 1234567;100;99;101;2\n";
        var r = await MarketBatchInspection.InspectAsync(new[] { Source("MES 09-26.Last.txt", replay), Source("MES 09-26.Bid.txt", replay) });
        Assert.Equal(99m, r.Files[0].Data!.Rows![0].Bid);
        Assert.Equal(MarketTextKind.TickReplay, r.Files[0].Data!.Kind);
        Assert.Equal(MarketFileState.Rejected, r.Files[1].State);
    }

    [Fact]
    public async Task CrcMismatchInvalidatesArchiveInsteadOfPublishingModifiedRows()
    {
        var original = Zip(("MES 09-26.Last.txt", Minute));
        using var stream = await original.Open(); using var memory = new MemoryStream(); await stream.CopyToAsync(memory);
        var bytes = memory.ToArray();
        for (var i = 0; i < bytes.Length - 20; i++)
            if (bytes[i] == 0x50 && bytes[i + 1] == 0x4b && bytes[i + 2] == 1 && bytes[i + 3] == 2) { bytes[i + 16] ^= 1; break; }
        var r = await MarketBatchInspection.InspectAsync(new[] { new MarketBatchSource("bad.zip", () => Task.FromResult<Stream>(new MemoryStream(bytes))) });
        Assert.False(r.Completed); Assert.Empty(r.Files);
    }

    [Fact]
    public async Task FileCountLimitIsEnforcedBeforeOpeningAnyInput()
    {
        var r = await MarketBatchInspection.InspectAsync(Enumerable.Range(0, 65).Select(_ => new MarketBatchSource("file.txt", () => throw new InvalidOperationException("Must not open"))));
        Assert.Equal("QF-BATCH-SELECTION-LIMIT", r.Code); Assert.Empty(r.Files);
    }

    [Fact]
    public async Task Utf8AndMixedFormatsAreStrictAndReturnNoRows()
    {
        using var bytes = new MemoryStream(new byte[] { 0xc3, 0x28 });
        Assert.Equal("QF-BATCH-ENCODING", (await MarketTextReader.ReadAsync(bytes)).Code);
        using var mixed = new MemoryStream(Encoding.UTF8.GetBytes(Minute + Day));
        var r = await MarketTextReader.ReadAsync(mixed); Assert.False(r.Complete); Assert.Null(r.Rows);
    }
}
