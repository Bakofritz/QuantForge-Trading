using System.Text;
using System.Text.Json;
namespace QuantForge.Core;
public sealed class ProductResearchComparisonFileStore
{
 private readonly string _path; public ProductResearchComparisonFileStore(string path)=>_path=Path.GetFullPath(path);
 public void Save(ProductResearchComparison value){if(value.DeclaresWinner)throw new InvalidOperationException("Research comparison cannot persist a winner declaration.");Directory.CreateDirectory(Path.GetDirectoryName(_path)!);File.WriteAllText(_path,JsonSerializer.Serialize(value,new JsonSerializerOptions{WriteIndented=true}),new UTF8Encoding(false));}
 public ProductResearchComparison Load(){var value=JsonSerializer.Deserialize<ProductResearchComparison>(File.ReadAllText(_path))??throw new InvalidOperationException("Research comparison JSON is invalid.");if(value.DeclaresWinner)throw new InvalidOperationException("Persisted research comparison contains a winner declaration.");return value;}
}
