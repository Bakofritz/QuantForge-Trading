using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchApplicationWorkflowTests
{
    [Fact]
    public void Application_workflow_requires_strategy_provenance_for_publication()
    {
        var request = new ResearchApplicationWorkflowRequest(
            new EndToEndResearchRequest(
                new ResearchWorkflowRequest(
                    new ResearchBatchRequest(ResearchBatchMode.ReadOnlyResearch, Array.Empty<ResearchRunRequest>()),
                    Array.Empty<DataReliabilityAssessment>(),
                    new[] { new SessionCoverageReport("dataset", "policy", 0, 0, 0, 0, Array.Empty<DateTimeOffset>(), Array.Empty<DateTimeOffset>(), true, "authoritative") }),
                new DatasetCatalog(),
                Array.Empty<StrategyAdmissionEnvelope>()),
            new Dictionary<string, ProvenanceRecord>());

        Assert.Throws<InvalidOperationException>(() => ResearchApplicationWorkflow.Run(request));
    }
}
