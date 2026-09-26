using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ResearchWorkspaceRebuilderTests
{[Fact] public void Merge_is_idempotent(){var e=ResearchWorkspaceIndexRules.From(TestFixtures.Gates());var i=ResearchWorkspaceIndexRules.Create(new[]{e});Assert.Single(ResearchWorkspaceRebuilder.Merge(i,new[]{e}).Entries);}}
