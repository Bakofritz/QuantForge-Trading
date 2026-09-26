using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ResearchLaunchRequestTests
{[Fact] public void Request_binds_ready_evidence_to_launch(){var g=TestFixtures.Gates();var l=ResearchLaunchIntentRules.Create(g,TestFixtures.Job(g));var r=ResearchLaunchRequestRules.Create(ResearchReadinessSnapshotRules.Create(g),l);Assert.False(string.IsNullOrWhiteSpace(r.RequestFingerprint));}}
