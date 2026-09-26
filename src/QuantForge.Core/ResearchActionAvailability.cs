namespace QuantForge.Core;
public sealed record ResearchActionAvailability(bool CanPrepareLaunch,bool CanReviewOutcomes,bool CanCompareOutcomes,bool CanExportEvidence,bool CanSubmitOrders,bool CanEnableLiveAccount);
public static class ResearchActionAvailabilityRules
{
 public static ResearchActionAvailability Create(ProductResearchWorkspaceDashboard dashboard){ArgumentNullException.ThrowIfNull(dashboard);return new(dashboard.Consistent&&dashboard.Gates>0,dashboard.Consistent&&dashboard.Outcomes>0,dashboard.Consistent&&dashboard.Outcomes>1,dashboard.Consistent,false,false);}
}
