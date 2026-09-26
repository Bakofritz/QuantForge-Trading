using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ProductResearchReviewFileStoreTests
{[Fact] public void Review_store_rejects_order_authority(){var p=Path.GetTempFileName();try{var s=new ProductResearchReviewFileStore(p);var v=new ProductResearchReview("j",ResearchResultStatus.Invalid,"d","s",null,"x",false,true);Assert.Throws<InvalidOperationException>(()=>s.Save(v));}finally{File.Delete(p);}}}
