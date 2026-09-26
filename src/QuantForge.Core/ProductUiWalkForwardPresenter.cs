namespace QuantForge.Core;

public sealed record ProductUiWalkForwardSegmentState(
    string SegmentId,
    string OptimizationFingerprint,
    string? SelectedTrainingJobId,
    ResearchResultStatus? EvaluationStatus,
    string? EvaluationJobId,
    decimal? FinalEquity,
    decimal? RealizedPnl,
    string SegmentFingerprint,
    string? Message);

public sealed record ProductUiWalkForwardState(
    string ResultFingerprint,
    bool Complete,
    string? BlockReason,
    bool LiveAccountEnabled,
    bool CanSubmitOrders,
    bool CanChangeApplicationSettings,
    IReadOnlyList<ProductUiWalkForwardSegmentState> Segments);

public static class ProductUiWalkForwardPresenter
{
    public static ProductUiWalkForwardState Create(WalkForwardResearchResult result)
    {
        WalkForwardResearchResultRules.Validate(result);
        var segments = result.Segments.Select(segment =>
        {
            var optimization = ProductUiOptimizationResultPresenter.Create(segment.Optimization);
            var evaluation = segment.Evaluation;
            return new ProductUiWalkForwardSegmentState(
                segment.SegmentId,
                optimization.ResultFingerprint,
                optimization.SelectedJobId,
                evaluation?.Status,
                evaluation?.JobId,
                evaluation?.Account?.Equity,
                evaluation?.Account?.RealizedPnl,
                segment.SegmentFingerprint,
                evaluation?.BlockReason ?? (!optimization.Complete ? "Training optimization did not complete." : null));
        }).ToArray();
        return new ProductUiWalkForwardState(
            result.ResultFingerprint,
            result.Complete,
            result.BlockReason,
            LiveAccountEnabled: false,
            CanSubmitOrders: false,
            CanChangeApplicationSettings: false,
            segments);
    }
}
