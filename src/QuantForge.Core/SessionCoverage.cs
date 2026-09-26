using System.Security.Cryptography;
using System.Text;

namespace QuantForge.Core;

/// <summary>Explicit, caller-supplied UTC session interval. It does not infer an exchange calendar.</summary>
public readonly record struct SessionInterval(DateTimeOffset Start, DateTimeOffset End)
{
    public void Validate()
    {
        if (End <= Start)
            throw new InvalidOperationException("Session interval end must be after its start.");
        if (Start.Offset != TimeSpan.Zero || End.Offset != TimeSpan.Zero)
            throw new InvalidOperationException("Session intervals must use explicit UTC timestamps.");
    }
}

/// <summary>
/// Authoritative session evidence supplied by a trusted calendar/source. The pipeline never invents sessions.
/// </summary>
public sealed record SessionCoveragePolicy(
    string PolicyId,
    string Version,
    string ProvenanceFingerprint,
    IReadOnlyList<SessionInterval> Sessions,
    bool Authoritative);

public sealed record SessionCoverageReport(
    string DatasetFingerprint,
    string PolicyFingerprint,
    int ExpectedMinuteCount,
    int ObservedInSessionMinuteCount,
    int MissingMinuteCount,
    int OutsideSessionMinuteCount,
    IReadOnlyList<DateTimeOffset> MissingMinutes,
    IReadOnlyList<DateTimeOffset> OutsideSessionMinutes,
    bool Authoritative,
    string Limitation)
{
    public bool Complete => MissingMinuteCount == 0 && OutsideSessionMinuteCount == 0;
    public bool ResearchAdmissible => Authoritative && Complete;
}

public static class SessionCoverageRules
{
    public const int MaximumExpectedMinutes = 5_000_000;

    public static SessionCoverageReport Analyze(
        string datasetFingerprint,
        IReadOnlyList<MarketEvent> bars,
        SessionCoveragePolicy policy)
    {
        if (string.IsNullOrWhiteSpace(datasetFingerprint))
            throw new InvalidOperationException("Session coverage requires dataset identity.");
        if (bars is null || bars.Count == 0)
            throw new InvalidOperationException("Session coverage requires observed market bars.");
        ArgumentNullException.ThrowIfNull(policy);

        ValidatePolicy(policy);

        var expected = new HashSet<DateTimeOffset>();
        foreach (var session in policy.Sessions)
        {
            session.Validate();
            for (var stamp = session.Start; stamp < session.End; stamp = stamp.AddMinutes(1))
            {
                if (expected.Count == MaximumExpectedMinutes)
                    throw new InvalidOperationException("Session coverage expected-minute limit exceeded.");
                expected.Add(stamp);
            }
        }

        var observed = new HashSet<DateTimeOffset>();
        foreach (var bar in bars)
        {
            bar.Validate();
            if (bar.Timestamp.Offset != TimeSpan.Zero || bar.Timestamp.Second != 0)
                throw new InvalidOperationException("Session coverage requires UTC minute bars.");
            observed.Add(bar.Timestamp);
        }

        var missing = expected.Where(x => !observed.Contains(x)).OrderBy(x => x).ToArray();
        var outside = observed.Where(x => !expected.Contains(x)).OrderBy(x => x).ToArray();
        var inSession = observed.Count - outside.Length;

        return new(
            datasetFingerprint,
            Fingerprint(policy),
            expected.Count,
            inSession,
            missing.Length,
            outside.Length,
            missing,
            outside,
            policy.Authoritative,
            policy.Authoritative
                ? "Explicit UTC session policy supplied; no exchange calendar was inferred."
                : "Observed coverage only; policy is not authoritative and cannot admit research.");
    }

    public static void RequireResearchAdmissible(SessionCoverageReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        if (!report.ResearchAdmissible)
            throw new InvalidOperationException(
                "Research admission requires authoritative session coverage with no missing or outside-session minutes.");
    }

    private static void ValidatePolicy(SessionCoveragePolicy policy)
    {
        if (string.IsNullOrWhiteSpace(policy.PolicyId) ||
            string.IsNullOrWhiteSpace(policy.Version) ||
            string.IsNullOrWhiteSpace(policy.ProvenanceFingerprint))
            throw new InvalidOperationException("Session policy identity and provenance are required.");
        if (policy.Sessions is null || policy.Sessions.Count == 0)
            throw new InvalidOperationException("Session coverage requires at least one explicit session interval.");

        var ordered = policy.Sessions.OrderBy(x => x.Start).ToArray();
        for (var i = 0; i < ordered.Length; i++)
        {
            ordered[i].Validate();
            if (i > 0 && ordered[i].Start < ordered[i - 1].End)
                throw new InvalidOperationException("Session intervals cannot overlap.");
        }
    }

    private static string Fingerprint(SessionCoveragePolicy policy)
    {
        var payload = new StringBuilder()
            .Append(policy.PolicyId).Append('|')
            .Append(policy.Version).Append('|')
            .Append(policy.ProvenanceFingerprint).Append('|')
            .Append(policy.Authoritative).Append('\n');
        foreach (var session in policy.Sessions.OrderBy(x => x.Start))
            payload.Append(session.Start.ToUniversalTime().ToString("O")).Append('|')
                .Append(session.End.ToUniversalTime().ToString("O")).Append('\n');
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload.ToString())));
    }
}
