using System.Text;
using System.Text.Json;
namespace QuantForge.Core;
public sealed class ResearchLaunchRequestFileStore
{
 private readonly string _path; public ResearchLaunchRequestFileStore(string path)=>_path=Path.GetFullPath(path);
 public void Save(ResearchLaunchRequest value){if(string.IsNullOrWhiteSpace(value.RequestFingerprint))throw new InvalidOperationException("Launch request fingerprint is required.");Directory.CreateDirectory(Path.GetDirectoryName(_path)!);File.WriteAllText(_path,JsonSerializer.Serialize(value,new JsonSerializerOptions{WriteIndented=true}),new UTF8Encoding(false));}
 public ResearchLaunchRequest Load(){return JsonSerializer.Deserialize<ResearchLaunchRequest>(File.ReadAllText(_path))??throw new InvalidOperationException("Launch request JSON is invalid.");}
}
