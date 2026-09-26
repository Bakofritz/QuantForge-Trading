using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core;

/// <summary>Persistable device-test progress bound to one exact app/platform identity. Never research admission.</summary>
public sealed record AndroidAcceptanceSnapshot(
    int SchemaVersion,
    string Version,
    string Build,
    string Platform,
    string OperatingSystem,
    AndroidTestMilestoneState MixedBatchInspection,
    AndroidTestMilestoneState CrossValidation,
    AndroidTestMilestoneState MinuteInspection,
    AndroidTestMilestoneState SourceComparison,
    string Fingerprint)
{
    public const int CurrentSchemaVersion = 1;

    public IReadOnlyDictionary<AndroidTestMilestone, AndroidTestMilestoneState> States =>
        new Dictionary<AndroidTestMilestone, AndroidTestMilestoneState>
        {
            [AndroidTestMilestone.MixedBatchInspection] = MixedBatchInspection,
            [AndroidTestMilestone.CrossValidation] = CrossValidation,
            [AndroidTestMilestone.MinuteInspection] = MinuteInspection,
            [AndroidTestMilestone.SourceComparison] = SourceComparison
        };

    public static AndroidAcceptanceSnapshot Create(
        AndroidBuildIdentity identity,
        IReadOnlyDictionary<AndroidTestMilestone, AndroidTestMilestoneState> states)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(states);
        ValidateIdentity(identity.Version, identity.Build, identity.Platform, identity.OperatingSystem);
        ValidateStates(states);
        var canonical = Canonical(identity.Version, identity.Build, identity.Platform, identity.OperatingSystem, states);
        return new AndroidAcceptanceSnapshot(
            CurrentSchemaVersion,
            identity.Version,
            identity.Build,
            identity.Platform,
            identity.OperatingSystem,
            states[AndroidTestMilestone.MixedBatchInspection],
            states[AndroidTestMilestone.CrossValidation],
            states[AndroidTestMilestone.MinuteInspection],
            states[AndroidTestMilestone.SourceComparison],
            Hash(canonical));
    }

    public bool Matches(AndroidBuildIdentity identity) =>
        identity is not null &&
        string.Equals(Version, identity.Version, StringComparison.Ordinal) &&
        string.Equals(Build, identity.Build, StringComparison.Ordinal) &&
        string.Equals(Platform, identity.Platform, StringComparison.Ordinal) &&
        string.Equals(OperatingSystem, identity.OperatingSystem, StringComparison.Ordinal);

    public bool IsValid()
    {
        if (SchemaVersion != CurrentSchemaVersion) return false;
        try
        {
            ValidateIdentity(Version, Build, Platform, OperatingSystem);
            ValidateStates(States);
            return string.Equals(Fingerprint, Hash(Canonical(Version, Build, Platform, OperatingSystem, States)), StringComparison.Ordinal);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public string Serialize()
    {
        if (!IsValid()) throw new InvalidOperationException("Android acceptance snapshot is invalid.");
        return Canonical(Version, Build, Platform, OperatingSystem, States) + $"Fingerprint={Fingerprint}\n";
    }

    public static bool TryParse(string? text, out AndroidAcceptanceSnapshot? snapshot)
    {
        snapshot = null;
        if (string.IsNullOrWhiteSpace(text)) return false;
        var lines = text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length != 10 || lines[0] != "QF-ANDROID-ACCEPTANCE/1") return false;
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var i = 1; i < lines.Length; i++)
        {
            var separator = lines[i].IndexOf('=');
            if (separator <= 0 || separator == lines[i].Length - 1) return false;
            if (!values.TryAdd(lines[i][..separator], lines[i][(separator + 1)..])) return false;
        }
        if (!values.TryGetValue("Version", out var version) ||
            !values.TryGetValue("Build", out var build) ||
            !values.TryGetValue("Platform", out var platform) ||
            !values.TryGetValue("OperatingSystem", out var operatingSystem) ||
            !TryState(values, "MixedBatchInspection", out var mixed) ||
            !TryState(values, "CrossValidation", out var cross) ||
            !TryState(values, "MinuteInspection", out var minute) ||
            !TryState(values, "SourceComparison", out var comparison) ||
            !values.TryGetValue("Fingerprint", out var fingerprint))
            return false;
        var candidate = new AndroidAcceptanceSnapshot(CurrentSchemaVersion, version, build, platform, operatingSystem,
            mixed, cross, minute, comparison, fingerprint);
        if (!candidate.IsValid()) return false;
        snapshot = candidate;
        return true;
    }

    private static string Canonical(string version, string build, string platform, string operatingSystem,
        IReadOnlyDictionary<AndroidTestMilestone, AndroidTestMilestoneState> states) =>
        "QF-ANDROID-ACCEPTANCE/1\n" +
        $"Version={version}\n" +
        $"Build={build}\n" +
        $"Platform={platform}\n" +
        $"OperatingSystem={operatingSystem}\n" +
        $"MixedBatchInspection={states[AndroidTestMilestone.MixedBatchInspection]}\n" +
        $"CrossValidation={states[AndroidTestMilestone.CrossValidation]}\n" +
        $"MinuteInspection={states[AndroidTestMilestone.MinuteInspection]}\n" +
        $"SourceComparison={states[AndroidTestMilestone.SourceComparison]}\n";

    private static string Hash(string canonical) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();

    private static bool TryState(IReadOnlyDictionary<string, string> values, string key, out AndroidTestMilestoneState state)
    {
        state = default;
        return values.TryGetValue(key, out var raw) && Enum.TryParse(raw, false, out state) && Enum.IsDefined(state);
    }

    private static void ValidateIdentity(params string[] values)
    {
        if (values.Any(value => string.IsNullOrWhiteSpace(value) || value.Length > 128 || value.Contains('\n') || value.Contains('\r')))
            throw new ArgumentException("Android build identity fields must be bounded single-line values.");
    }

    private static void ValidateStates(IReadOnlyDictionary<AndroidTestMilestone, AndroidTestMilestoneState> states)
    {
        var milestones = Enum.GetValues<AndroidTestMilestone>();
        if (states.Count != milestones.Length || milestones.Any(milestone => !states.TryGetValue(milestone, out var state) || !Enum.IsDefined(state)))
            throw new ArgumentException("Android acceptance snapshot requires every valid milestone state.");
    }
}
