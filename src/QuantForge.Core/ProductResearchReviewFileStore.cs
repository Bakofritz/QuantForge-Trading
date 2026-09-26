using System.Text;
using System.Text.Json;
namespace QuantForge.Core;
public sealed class ProductResearchReviewFileStore
{
 private readonly string _path; public ProductResearchReviewFileStore(string path)=>_path=Path.GetFullPath(path);
 public void Save(ProductResearchReview value){if(value.CanSubmitOrders)throw new InvalidOperationException("Research review cannot carry order authority.");Directory.CreateDirectory(Path.GetDirectoryName(_path)!);File.WriteAllText(_path,JsonSerializer.Serialize(value,new JsonSerializerOptions{WriteIndented=true}),new UTF8Encoding(false));}
 public ProductResearchReview Load(){var value=JsonSerializer.Deserialize<ProductResearchReview>(File.ReadAllText(_path))??throw new InvalidOperationException("Research review JSON is invalid.");if(value.CanSubmitOrders)throw new InvalidOperationException("Persisted research review contains forbidden authority.");return value;}
}
