using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ResearchWorkspaceSnapshotTests
{[Fact] public void Snapshot_is_fingerprinted(){var g=TestFixtures.Gates();var i=ResearchWorkspaceIndexRules.Create(new[]{ResearchWorkspaceIndexRules.From(g)});var s=ResearchWorkspaceSnapshotRules.Create(i,DateTimeOffset.Parse("2026-01-01T00:00:00Z"));ResearchWorkspaceSnapshotRules.Validate(s);}}
