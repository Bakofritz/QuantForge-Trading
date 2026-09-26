using System.Text;
using System.Text.Json;
namespace QuantForge.Core;
public sealed class ProductResearchWorkspaceDashboardFileStore
{
 private readonly string _path; public ProductResearchWorkspaceDashboardFileStore(string path)=>_path=Path.GetFullPath(path);
 public void Save(ProductResearchWorkspaceDashboard value){Validate(value);Directory.CreateDirectory(Path.GetDirectoryName(_path)!);File.WriteAllText(_path,JsonSerializer.Serialize(value,new JsonSerializerOptions{WriteIndented=true}),new UTF8Encoding(false));}
 public ProductResearchWorkspaceDashboard Load(){var value=JsonSerializer.Deserialize<ProductResearchWorkspaceDashboard>(File.ReadAllText(_path))??throw new InvalidOperationException("Workspace dashboard JSON is invalid.");Validate(value);return value;}
 private static void Validate(ProductResearchWorkspaceDashboard value){if(string.IsNullOrWhiteSpace(value.WorkspaceFingerprint))throw new InvalidOperationException("Workspace dashboard identity is required.");if(value.LiveAccountEnabled||value.CanSubmitOrders)throw new InvalidOperationException("Research dashboard cannot contain live authority.");}
}
