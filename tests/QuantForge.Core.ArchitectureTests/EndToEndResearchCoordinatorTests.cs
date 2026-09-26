using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class EndToEndResearchCoordinatorTests
{
    [Fact]
    public void Research_admitted_path_requires_session_coverage()
    {
        var request = new EndToEndResearchRequest(
            new ResearchWorkflowRequest(
                new ResearchBatchRequest(ResearchBatchMode.ReadOnlyResearch, Array.Empty<ResearchRunRequest>()),
                Array.Empty<DataReliabilityAssessment>()),
            new DatasetCatalog(),
            Array.Empty<StrategyAdmissionEnvelope>());

        Assert.Throws<InvalidOperationException>(() => EndToEndResearchCoordinator.RunResearchAdmitted(request));
    }
}
