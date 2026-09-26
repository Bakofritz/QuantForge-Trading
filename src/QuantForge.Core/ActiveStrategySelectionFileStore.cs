using System.Text;
using System.Text.Json;
namespace QuantForge.Core;
public sealed class ActiveStrategySelectionFileStore
{
 private readonly string _path; public ActiveStrategySelectionFileStore(string path)=>_path=Path.GetFullPath(path);
 public void Save(ActiveStrategySelection value){if(string.IsNullOrWhiteSpace(value.SelectionFingerprint)||string.IsNullOrWhiteSpace(value.StrategyArtifactFingerprint))throw new InvalidOperationException("Strategy selection identity is incomplete.");Directory.CreateDirectory(Path.GetDirectoryName(_path)!);File.WriteAllText(_path,JsonSerializer.Serialize(value,new JsonSerializerOptions{WriteIndented=true}),new UTF8Encoding(false));}
 public ActiveStrategySelection Load(){return JsonSerializer.Deserialize<ActiveStrategySelection>(File.ReadAllText(_path))??throw new InvalidOperationException("Strategy selection JSON is invalid.");}
}
