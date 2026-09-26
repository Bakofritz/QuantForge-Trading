namespace QuantForge.Core;
public sealed record ProductResearchComparison(string DatasetFingerprint,string LeftOutcomeFingerprint,string RightOutcomeFingerprint,ResearchResultStatus LeftStatus,ResearchResultStatus RightStatus,decimal? SimulatedEquityDifference,bool DeclaresWinner);
public static class ProductResearchComparisonRules
{
 public static ProductResearchComparison Create(ResearchOutcomeComparison comparison)
 {
  ArgumentNullException.ThrowIfNull(comparison);
  if (string.IsNullOrWhiteSpace(comparison.ComparisonFingerprint) || string.IsNullOrWhiteSpace(comparison.DatasetFingerprint))
   throw new InvalidOperationException("Research comparison identity is incomplete.");
  return new(comparison.DatasetFingerprint,comparison.LeftOutcomeFingerprint,comparison.RightOutcomeFingerprint,comparison.LeftStatus,comparison.RightStatus,comparison.EquityDifference,false);
 }
}
