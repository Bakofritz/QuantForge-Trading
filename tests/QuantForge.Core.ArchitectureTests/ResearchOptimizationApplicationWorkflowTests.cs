using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchOptimizationApplicationWorkflowTests
{
    [Fact]
    public void Optimization_application_requires_authoritative_session_evidence()
    {
        var request = new ResearchOptimizationApplicationRequest(
            new ResearchOptimizationPlan(Array.Empty<ResearchRunRequest>()),
            new DatasetCatalog(), Array.Empty<StrategyAdmissionEnvelope>(),
            Array.Empty<DataReliabilityAssessment>(), Array.Empty<SessionCoverageReport>());
        Assert.Throws<InvalidOperationException>(() => ResearchOptimizationApplicationWorkflow.Run(request, ResearchOptimizationObjective.FinalEquity));
    }
}
