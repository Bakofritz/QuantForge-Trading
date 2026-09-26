namespace QuantForge.Core;

public sealed record WalkForwardApplicationWorkflowResult(
    WalkForwardResearchResult Research,
    ProductUiWalkForwardState State,
    string PersistedPath);

public static class WalkForwardApplicationWorkflow
{
    public static WalkForwardApplicationWorkflowResult RunAndPersist(
        WalkForwardResearchPlan plan,
        string rootDirectory)
    {
        var result = WalkForwardResearchRunner.Run(plan);
        WalkForwardResearchResultRules.Validate(result);
        var state = ProductUiWalkForwardPresenter.Create(result);
        var path = new WalkForwardResearchFileStore(rootDirectory).Save(result);
        return new WalkForwardApplicationWorkflowResult(result, state, path);
    }

    public static WalkForwardApplicationWorkflowResult Recover(
        string rootDirectory,
        string resultFingerprint)
    {
        var result = new WalkForwardResearchFileStore(rootDirectory).Load(resultFingerprint);
        var state = ProductUiWalkForwardPresenter.Create(result);
        var path = Path.Combine(Path.GetFullPath(rootDirectory), resultFingerprint, "walk-forward.json");
        return new WalkForwardApplicationWorkflowResult(result, state, path);
    }
}
