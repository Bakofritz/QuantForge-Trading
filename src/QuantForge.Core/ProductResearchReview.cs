namespace QuantForge.Core;
public sealed record ProductResearchReview(string JobFingerprint,ResearchResultStatus Status,string DatasetFingerprint,string StrategyFingerprint,decimal? EndingEquity,string? Message,bool CanExport,bool CanSubmitOrders);
public static class ProductResearchReviewRules
{
 public static ProductResearchReview Create(ResearchOutcomeArtifact outcome)
 {
  ArgumentNullException.ThrowIfNull(outcome);
  ResearchReportRules.Validate(outcome.Report);
  if (string.IsNullOrWhiteSpace(outcome.ArtifactFingerprint)) throw new InvalidOperationException("Research outcome fingerprint is required.");
  var complete=outcome.Report.Status==ResearchResultStatus.Complete;
  return new(outcome.Report.JobId,outcome.Report.Status,outcome.Report.DatasetFingerprint,outcome.Report.StrategyFingerprint,outcome.Report.EndingEquity,outcome.Report.Message,complete,false);
 }
}
