using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ProductResearchComparisonTests
{[Fact] public void Product_comparison_never_declares_winner(){Assert.False(new ProductResearchComparison("d","a","b",ResearchResultStatus.Invalid,ResearchResultStatus.Invalid,null,false).DeclaresWinner);}}
