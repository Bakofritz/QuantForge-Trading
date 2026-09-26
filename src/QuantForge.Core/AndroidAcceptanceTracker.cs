namespace QuantForge.Core;

public enum AndroidTestMilestone
{
    MixedBatchInspection,
    CrossValidation,
    MinuteInspection,
    SourceComparison
}

public enum AndroidTestMilestoneState
{
    NotRun,
    Passed,
    Failed
}

public sealed class AndroidAcceptanceTracker
{
    private readonly Dictionary<AndroidTestMilestone, AndroidTestMilestoneState> _states =
        Enum.GetValues<AndroidTestMilestone>().ToDictionary(x => x, _ => AndroidTestMilestoneState.NotRun);

    public IReadOnlyDictionary<AndroidTestMilestone, AndroidTestMilestoneState> States => _states;

    public void Record(AndroidTestMilestone milestone, bool passed)
    {
        if (!Enum.IsDefined(milestone))
            throw new ArgumentOutOfRangeException(nameof(milestone));
        _states[milestone] = passed ? AndroidTestMilestoneState.Passed : AndroidTestMilestoneState.Failed;
    }

    public void Restore(IReadOnlyDictionary<AndroidTestMilestone, AndroidTestMilestoneState> states)
    {
        ArgumentNullException.ThrowIfNull(states);
        var milestones = Enum.GetValues<AndroidTestMilestone>();
        if (states.Count != milestones.Length || milestones.Any(milestone => !states.TryGetValue(milestone, out var state) || !Enum.IsDefined(state)))
            throw new ArgumentException("Every valid Android test milestone state is required.", nameof(states));
        foreach (var milestone in milestones) _states[milestone] = states[milestone];
    }

    public void Reset()
    {
        foreach (var milestone in Enum.GetValues<AndroidTestMilestone>())
            _states[milestone] = AndroidTestMilestoneState.NotRun;
    }

    public string Render()
    {
        var passed = _states.Count(x => x.Value == AndroidTestMilestoneState.Passed);
        var failed = _states.Count(x => x.Value == AndroidTestMilestoneState.Failed);
        var pending = _states.Count - passed - failed;
        var details = string.Join(" | ", _states.OrderBy(x => x.Key).Select(x => $"{Label(x.Key)}={x.Value}"));
        return $"Device test progress: {passed} passed, {failed} failed, {pending} not run. {details}. This is test-session status only; it is not research admission.";
    }

    private static string Label(AndroidTestMilestone milestone) => milestone switch
    {
        AndroidTestMilestone.MixedBatchInspection => "mixed batch",
        AndroidTestMilestone.CrossValidation => "cross-validation",
        AndroidTestMilestone.MinuteInspection => "minute inspection",
        AndroidTestMilestone.SourceComparison => "source comparison",
        _ => throw new ArgumentOutOfRangeException(nameof(milestone))
    };
}
