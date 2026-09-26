using System.Text;

namespace QuantForge.Core;

public static class AndroidDeviceTestReport
{
    public static string Render(AndroidBuildIdentity identity, IReadOnlyDictionary<AndroidTestMilestone, AndroidTestMilestoneState> states)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(states);
        if (states.Count != Enum.GetValues<AndroidTestMilestone>().Length || Enum.GetValues<AndroidTestMilestone>().Any(x => !states.ContainsKey(x)))
            throw new InvalidOperationException("Android device test report requires every milestone state.");

        var text = new StringBuilder();
        text.AppendLine("QuantForge Android Device Test Report");
        text.AppendLine($"Version: {identity.Version} ({identity.Build})");
        text.AppendLine($"Platform: {identity.Platform}");
        text.AppendLine($"OS: {identity.OperatingSystem}");
        foreach (var milestone in Enum.GetValues<AndroidTestMilestone>())
            text.AppendLine($"{milestone}: {states[milestone]}");
        text.AppendLine("Authority: test status only; no research admission, broker connection, live account, or order authority.");
        return text.ToString();
    }
}
