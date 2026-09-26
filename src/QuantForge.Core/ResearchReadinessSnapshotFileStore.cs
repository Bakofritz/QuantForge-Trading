using System.Text;
using System.Text.Json;
namespace QuantForge.Core;
public sealed class ResearchReadinessSnapshotFileStore
{
 private readonly string _path; public ResearchReadinessSnapshotFileStore(string path)=>_path=Path.GetFullPath(path);
 public void Save(ResearchReadinessSnapshot value){if(!value.Ready) throw new InvalidOperationException("Only complete readiness snapshots may be persisted as launch-ready evidence.");Directory.CreateDirectory(Path.GetDirectoryName(_path)!);File.WriteAllText(_path,JsonSerializer.Serialize(value,new JsonSerializerOptions{WriteIndented=true}),new UTF8Encoding(false));}
 public ResearchReadinessSnapshot Load(){var value=JsonSerializer.Deserialize<ResearchReadinessSnapshot>(File.ReadAllText(_path))??throw new InvalidOperationException("Readiness snapshot JSON is invalid.");if(string.IsNullOrWhiteSpace(value.Fingerprint)||!value.Ready)throw new InvalidOperationException("Persisted readiness snapshot is not launch-ready.");return value;}
}
