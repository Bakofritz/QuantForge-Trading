using System.Text;using System.Text.Json;
namespace QuantForge.Core;
public sealed class ResearchWorkspaceActivityFileStore
{private readonly string _path;public ResearchWorkspaceActivityFileStore(string path)=>_path=Path.GetFullPath(path);public void Save(IReadOnlyList<ResearchWorkspaceActivityItem> items){ArgumentNullException.ThrowIfNull(items);Directory.CreateDirectory(Path.GetDirectoryName(_path)!);File.WriteAllText(_path,JsonSerializer.Serialize(items,new JsonSerializerOptions{WriteIndented=true}),new UTF8Encoding(false));}public IReadOnlyList<ResearchWorkspaceActivityItem> Load(){return JsonSerializer.Deserialize<ResearchWorkspaceActivityItem[]>(File.ReadAllText(_path))??throw new InvalidOperationException("Workspace activity JSON is invalid.");}}
