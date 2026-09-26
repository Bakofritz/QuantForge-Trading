using System.IO.Compression;
using System.Text.Json;

namespace QuantForge.Core.ArchitectureTests;

public sealed class DiagnosticJournalTests
{
    private static DiagnosticEnvironment EnvironmentInfo => new("test-build", "0.30.33", "3033", "test-model", "test-maker", "16", 1080, 2400, 3);

    [Fact]
    public async Task ExportContainsOnlyFixedFilesAndExplicitUserNotes()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            var journal = new DiagnosticJournal(dir, EnvironmentInfo);
            Assert.False(journal.Record((DiagnosticAction)999));
            Assert.False(journal.Record(DiagnosticAction.DataInspection, userNote: "private-import-path"));
            Assert.True(journal.Record(DiagnosticAction.DataInspection, DiagnosticOutcome.Started));
            Assert.True(journal.Record(DiagnosticAction.ProblemMarked, userNote: "Button felt slow"));
            await journal.StopAsync();
            Assert.False(journal.Record(DiagnosticAction.DataInspection));
            Assert.False(journal.StorageFailed);
            var zip = Path.Combine(dir, "export.zip");
            await DiagnosticJournal.ExportAsync(dir, zip);
            using var archive = ZipFile.OpenRead(zip);
            Assert.Equal(4, archive.Entries.Count);
            using var reader = new StreamReader(archive.GetEntry("events.jsonl")!.Open());
            var text = await reader.ReadToEndAsync();
            Assert.Contains("Button felt slow", text);
            Assert.DoesNotContain("private-import-path", text);
            using var manifestReader = new StreamReader(archive.GetEntry("manifest.json")!.Open());
            using var manifest = JsonDocument.Parse(await manifestReader.ReadToEndAsync());
            Assert.False(manifest.RootElement.GetProperty("Interrupted").GetBoolean());
            Assert.Equal(64, manifest.RootElement.GetProperty("EventSha256").GetString()!.Length);
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public async Task EventAndByteBudgetsBoundFloodsWithoutThrowing()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            var journal = new DiagnosticJournal(dir, EnvironmentInfo);
            for (var i = 0; i < 10000; i++) journal.Record(DiagnosticAction.MemorySample);
            await journal.StopAsync();
            Assert.True(journal.DroppedEvents > 0);
            var path = Path.Combine(dir, "events.jsonl");
            Assert.True(new FileInfo(path).Length <= DiagnosticJournal.MaximumFileBytes);
            Assert.True(File.ReadAllLines(path).Length <= DiagnosticJournal.MaximumEvents);
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public async Task InterruptedJournalCanExportWithExplicitIncompleteTail()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            var journal = new DiagnosticJournal(dir, EnvironmentInfo);
            await journal.StopAsync();
            var path = Path.Combine(dir, "events.jsonl");
            var lines = File.ReadAllLines(path).Where(l => !l.Contains("SessionStopped", StringComparison.Ordinal));
            File.WriteAllText(path, string.Join('\n', lines) + "\n{partial");
            var zip = Path.Combine(dir, "export.zip");
            await DiagnosticJournal.ExportAsync(dir, zip);
            using var archive = ZipFile.OpenRead(zip);
            using var reader = new StreamReader(archive.GetEntry("manifest.json")!.Open());
            using var manifest = JsonDocument.Parse(await reader.ReadToEndAsync());
            Assert.True(manifest.RootElement.GetProperty("Interrupted").GetBoolean());
            Assert.Equal(1, manifest.RootElement.GetProperty("IncompleteLines").GetInt32());
        }
        finally { Directory.Delete(dir, true); }
    }
    [Fact]
    public async Task ExportCanEmbedBuildBoundAndroidAcceptanceEvidence()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            var journal = new DiagnosticJournal(dir, EnvironmentInfo);
            await journal.StopAsync();
            var tracker = new AndroidAcceptanceTracker();
            tracker.Record(AndroidTestMilestone.MixedBatchInspection, true);
            var identity = new AndroidBuildIdentity("0.30.33", "3033", "Android", "16");
            var snapshot = AndroidAcceptanceSnapshot.Create(identity, tracker.States);
            var zip = Path.Combine(dir, "export.zip");

            await DiagnosticJournal.ExportAsync(dir, zip, snapshot.Serialize());

            using var archive = ZipFile.OpenRead(zip);
            Assert.NotNull(archive.GetEntry("android-acceptance.txt"));
            using var manifestReader = new StreamReader(archive.GetEntry("manifest.json")!.Open());
            using var manifest = JsonDocument.Parse(await manifestReader.ReadToEndAsync());
            Assert.Equal(snapshot.Fingerprint, manifest.RootElement.GetProperty("AndroidAcceptanceFingerprint").GetString());
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public async Task ExportRejectsAcceptanceEvidenceFromDifferentBuild()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            var journal = new DiagnosticJournal(dir, EnvironmentInfo);
            await journal.StopAsync();
            var tracker = new AndroidAcceptanceTracker();
            var snapshot = AndroidAcceptanceSnapshot.Create(new AndroidBuildIdentity("0.30.33", "9999", "Android", "16"), tracker.States);
            var zip = Path.Combine(dir, "export.zip");
            await Assert.ThrowsAsync<InvalidOperationException>(() => DiagnosticJournal.ExportAsync(dir, zip, snapshot.Serialize()));
        }
        finally { Directory.Delete(dir, true); }
    }

}
