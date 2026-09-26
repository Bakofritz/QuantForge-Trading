using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ResearchReadinessSnapshotTests
{[Fact] public void Complete_gates_are_ready(){Assert.True(ResearchReadinessSnapshotRules.Create(TestFixtures.Gates()).Ready);}}
