using System.Text;
using System.Text.Json;
namespace QuantForge.Core;
public sealed class ResearchIdentityDiagnosticFileStore
{
 private readonly string _path; public ResearchIdentityDiagnosticFileStore(string path)=>_path=Path.GetFullPath(path);
 public void Save(ResearchIdentityDiagnostic value){Directory.CreateDirectory(Path.GetDirectoryName(_path)!);File.WriteAllText(_path,JsonSerializer.Serialize(value,new JsonSerializerOptions{WriteIndented=true}),new UTF8Encoding(false));}
 public ResearchIdentityDiagnostic Load(){return JsonSerializer.Deserialize<ResearchIdentityDiagnostic>(File.ReadAllText(_path))??throw new InvalidOperationException("Identity diagnostic JSON is invalid.");}
}
