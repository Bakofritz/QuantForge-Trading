using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;

namespace QuantForge.Core;

public enum DiagnosticAction
{
    SessionStarted, SessionStopped, ProblemMarked, ManifestInspection, DataInspection,
    SourceComparison, DataCleared, DeclarationChanged, CancelRequested, PageAppeared,
    PageDisappeared, WindowActivated, WindowDeactivated, WindowStopped, WindowResumed,
    MemorySample, ExportRequested, FileLabelCheck, ManifestProcessing, DataProcessing, ComparisonProcessing
}
public enum DiagnosticOutcome { Observed, Started, Completed, Cancelled, Invalid, Unavailable }
public sealed record DiagnosticEnvironment(string Build, string Model, string Manufacturer,
    string OperatingSystem, double ScreenWidth, double ScreenHeight, double Density);
public sealed record DiagnosticEvent(long Sequence, DateTimeOffset Utc, long ElapsedMilliseconds,
    DiagnosticAction Action, DiagnosticOutcome Outcome, long DurationMilliseconds,
    long ManagedMemoryBytes, string? UserNote);

/// <summary>Opt-in app diagnostics only. Never a research admission or authority token.</summary>
public sealed class DiagnosticJournal
{
    public const int MaximumEvents = 2000;
    public const int MaximumFileBytes = 2 * 1024 * 1024;
    private readonly Channel<DiagnosticEvent> _queue = Channel.CreateBounded<DiagnosticEvent>(256);
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly object _gate = new();
    private readonly Task _writer;
    private long _count;
    private long _dropped;
    private bool _stopped;
    private volatile bool _storageFailed;
    public bool StorageFailed => _storageFailed;
    public long DroppedEvents => Interlocked.Read(ref _dropped);

    // Directory must be the adapter's fixed private diagnostic folder, never an imported path.
    public DiagnosticJournal(string directory, DiagnosticEnvironment environment)
    {
        if (new[] { environment.Build, environment.Model, environment.Manufacturer, environment.OperatingSystem }
            .Any(value => string.IsNullOrWhiteSpace(value) || value.Length > 128))
            throw new ArgumentException("Invalid diagnostic environment.");
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "environment.json"), JsonSerializer.Serialize(environment, DiagnosticJsonContext.Default.DiagnosticEnvironment));
        _writer = Task.Run(async () =>
        {
            try
            {
                await using var stream = new FileStream(Path.Combine(directory, "events.jsonl"), FileMode.Create,
                    FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous);
                await foreach (var item in _queue.Reader.ReadAllAsync())
                {
                    var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(item, DiagnosticJsonContext.Default.DiagnosticEvent) + "\n");
                    if (stream.Position + bytes.Length > MaximumFileBytes) { Interlocked.Increment(ref _dropped); continue; }
                    await stream.WriteAsync(bytes);
                    await stream.FlushAsync();
                }
                await File.WriteAllTextAsync(Path.Combine(directory, "status.json"),
                    JsonSerializer.Serialize(new DiagnosticRecorderStatus(DroppedEvents, StorageFailed), DiagnosticJsonContext.Default.DiagnosticRecorderStatus));
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            { _storageFailed = true; }
        });
        Record(DiagnosticAction.SessionStarted);
    }

    public bool Record(DiagnosticAction action, DiagnosticOutcome outcome = DiagnosticOutcome.Observed,
        long durationMilliseconds = 0, string? userNote = null)
    {
        if (!Enum.IsDefined(action) || !Enum.IsDefined(outcome) || durationMilliseconds < 0 ||
            (userNote is not null && (action != DiagnosticAction.ProblemMarked || userNote.Length > 500)))
            return false;
        lock (_gate)
        {
            if (_stopped || _storageFailed) return false;
            if (_count >= MaximumEvents || (_count >= MaximumEvents - 1 && action != DiagnosticAction.SessionStopped))
            { Interlocked.Increment(ref _dropped); return false; }
            var item = new DiagnosticEvent(++_count, DateTimeOffset.UtcNow, _clock.ElapsedMilliseconds,
                action, outcome, durationMilliseconds, GC.GetTotalMemory(false), userNote);
            if (_queue.Writer.TryWrite(item)) return true;
            Interlocked.Increment(ref _dropped);
            return false;
        }
    }

    public async Task StopAsync()
    {
        lock (_gate)
        {
            if (!_stopped)
            {
                Record(DiagnosticAction.SessionStopped);
                _stopped = true;
                _queue.Writer.TryComplete();
            }
        }
        await _writer;
    }

    public static async Task ExportAsync(string directory, string destination)
    {
        // Caller stops recording before export. Read only these fixed diagnostic files.
        var eventPath = Path.Combine(directory, "events.jsonl");
        var environmentPath = Path.Combine(directory, "environment.json");
        if (new FileInfo(eventPath).Length > MaximumFileBytes || new FileInfo(environmentPath).Length > 4096)
            throw new InvalidOperationException("Diagnostic files exceed export limits.");
        var eventBytes = await File.ReadAllBytesAsync(eventPath);
        var environment = JsonSerializer.Deserialize(await File.ReadAllTextAsync(environmentPath), DiagnosticJsonContext.Default.DiagnosticEnvironment)
            ?? throw new InvalidOperationException("Missing diagnostic environment.");
        var events = new List<DiagnosticEvent>();
        var incompleteLines = 0;
        foreach (var line in Encoding.UTF8.GetString(eventBytes).Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                var item = JsonSerializer.Deserialize(line, DiagnosticJsonContext.Default.DiagnosticEvent);
                if (item is null) incompleteLines++; else events.Add(item);
            }
            catch (JsonException) { incompleteLines++; }
        }
        var interrupted = !events.Any(item => item.Action == DiagnosticAction.SessionStopped);
        var manifest = new DiagnosticExportManifest(1, environment, DateTimeOffset.UtcNow,
            events.Count, incompleteLines, interrupted, Convert.ToHexString(SHA256.HashData(eventBytes)),
            "App-only best-effort diagnostics. No guarantee of capturing fatal crashes, ANRs, final buffered events, CPU/battery or complete startup metrics. Not research evidence.");
        using var archive = ZipFile.Open(destination, ZipArchiveMode.Create);
        async Task Add(string name, string text)
        {
            await using var stream = archive.CreateEntry(name).Open();
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(text);
        }
        await Add("manifest.json", JsonSerializer.Serialize(manifest, DiagnosticJsonContext.Default.DiagnosticExportManifest));
        await Add("events.jsonl", Encoding.UTF8.GetString(eventBytes));
        var statusPath = Path.Combine(directory, "status.json");
        if (File.Exists(statusPath) && new FileInfo(statusPath).Length <= 4096)
            await Add("recorder-status.json", await File.ReadAllTextAsync(statusPath));
        await Add("summary.txt", $"QuantForge diagnostic test session\nBuild: {environment.Build}\nDevice: {environment.Manufacturer} {environment.Model}\nEvents: {events.Count}\nInterrupted or stop event unavailable: {interrupted}\nIncomplete lines: {incompleteLines}\n" +
            string.Join("\n", events.GroupBy(e => e.Action).Select(g => $"{g.Key}: {g.Count()}")) +
            "\nOnly manually entered problem notes may contain free text. No automatic upload. No trading authority.\n");
    }
}


internal sealed record DiagnosticRecorderStatus(long DroppedEvents, bool StorageFailed);
internal sealed record DiagnosticExportManifest(int SchemaVersion, DiagnosticEnvironment Environment,
    DateTimeOffset ExportedUtc, int EventCount, int IncompleteLines, bool Interrupted,
    string EventSha256, string Limitation);

[JsonSourceGenerationOptions(UseStringEnumConverter = true)]
[JsonSerializable(typeof(DiagnosticEnvironment))]
[JsonSerializable(typeof(DiagnosticEvent))]
[JsonSerializable(typeof(DiagnosticRecorderStatus))]
[JsonSerializable(typeof(DiagnosticExportManifest))]
internal partial class DiagnosticJsonContext : JsonSerializerContext { }
