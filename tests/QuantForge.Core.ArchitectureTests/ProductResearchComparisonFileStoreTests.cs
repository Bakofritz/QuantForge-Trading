using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ProductResearchComparisonFileStoreTests
{[Fact] public void Winner_declaration_is_rejected(){var p=Path.GetTempFileName();try{var s=new ProductResearchComparisonFileStore(p);Assert.Throws<InvalidOperationException>(()=>s.Save(new ProductResearchComparison("d","l","r",ResearchResultStatus.Complete,ResearchResultStatus.Complete,1m,true)));}finally{File.Delete(p);}}}
