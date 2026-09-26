namespace QuantForge.Core;

/// <summary>
/// Exact Android app/platform identity used to bind native validation and device-test evidence.
/// This is identity metadata only and grants no research, account, broker, or order authority.
/// </summary>
public sealed record AndroidBuildIdentity(
    string Version,
    string Build,
    string Platform,
    string OperatingSystem)
{
    public AndroidBuildIdentity
    {
        Validate(nameof(Version), Version);
        Validate(nameof(Build), Build);
        Validate(nameof(Platform), Platform);
        Validate(nameof(OperatingSystem), OperatingSystem);
    }

    private static void Validate(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 128 || value.Contains('\n') || value.Contains('\r'))
            throw new ArgumentException("Android build identity fields must be bounded single-line values.", name);
    }
}
