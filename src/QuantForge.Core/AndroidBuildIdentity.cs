namespace QuantForge.Core;

/// <summary>
/// Exact Android app/platform identity used to bind native validation and device-test evidence.
/// Identity metadata only; grants no research, account, broker, or order authority.
/// </summary>
public sealed record AndroidBuildIdentity
{
    public AndroidBuildIdentity(string version, string build, string platform, string operatingSystem)
    {
        Validate(nameof(version), version);
        Validate(nameof(build), build);
        Validate(nameof(platform), platform);
        Validate(nameof(operatingSystem), operatingSystem);
        Version = version;
        Build = build;
        Platform = platform;
        OperatingSystem = operatingSystem;
    }

    public string Version { get; }
    public string Build { get; }
    public string Platform { get; }
    public string OperatingSystem { get; }

    private static void Validate(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 128 || value.Contains('\n') || value.Contains('\r'))
            throw new ArgumentException("Android build identity fields must be bounded single-line values.", name);
    }
}
