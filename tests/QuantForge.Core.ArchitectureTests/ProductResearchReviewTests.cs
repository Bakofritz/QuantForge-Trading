using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ProductResearchReviewTests
{[Fact] public void Review_never_grants_order_authority(){var g=TestFixtures.Gates();var l=ResearchLaunchIntentRules.Create(g,TestFixtures.Job(g));var id=l.Job.Identity;var r=new ResearchReport(id.JobFingerprint,ResearchResultStatus.DataBlocked,id.DatasetFingerprint,id.StrategyFingerprint,id.ExecutionPolicyFingerprint,id.ParameterSetFingerprint,id.TemporalPartitionId,"blocked",null,null);var o=ResearchOutcomeArtifactRules.Create(l,r);Assert.False(ProductResearchReviewRules.Create(o).CanSubmitOrders);}}
