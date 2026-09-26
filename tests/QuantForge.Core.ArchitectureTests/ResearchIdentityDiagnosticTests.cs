using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ResearchIdentityDiagnosticTests
{[Fact] public void Matching_ready_launch_has_no_differences(){var g=TestFixtures.Gates();var r=ResearchReadinessSnapshotRules.Create(g);var l=ResearchLaunchIntentRules.Create(g,TestFixtures.Job(g));Assert.True(ResearchIdentityDiagnosticRules.Compare(r,l).Matches);}}
