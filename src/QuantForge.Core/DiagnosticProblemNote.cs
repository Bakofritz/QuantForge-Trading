namespace QuantForge.Core;

public static class DiagnosticProblemNote
{
    public const int MaximumFieldLength = 200;

    public static string Compose(string? expected, string? actual)
    {
        expected = Normalize(expected);
        actual = Normalize(actual);
        if (expected.Length == 0 && actual.Length == 0)
            throw new ArgumentException("Expected or actual behavior is required.");
        if (expected.Length > MaximumFieldLength || actual.Length > MaximumFieldLength)
            throw new ArgumentException($"Expected/actual behavior must be {MaximumFieldLength} characters or fewer per field.");
        return $"Expected: {(expected.Length == 0 ? "(not supplied)" : expected)}\nActual: {(actual.Length == 0 ? "(not supplied)" : actual)}";
    }

    private static string Normalize(string? value) => (value ?? string.Empty).Trim().Replace("\r\n", " ").Replace('\r', ' ').Replace('\n', ' ');
}
