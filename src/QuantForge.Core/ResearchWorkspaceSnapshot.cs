using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
namespace QuantForge.Core;
public sealed record ResearchWorkspaceSnapshot(string SnapshotFingerprint,string IndexFingerprint,DateTimeOffset CreatedUtc,IReadOnlyList<ResearchWorkspaceEntry> Entries);
public static class ResearchWorkspaceSnapshotRules
{
 public static ResearchWorkspaceSnapshot Create(ResearchWorkspaceIndex index,DateTimeOffset createdUtc)
 {
  ResearchWorkspaceIndexRules.Validate(index);
  var stamp=createdUtc.ToUniversalTime();
  var text=index.IndexFingerprint+"|"+stamp.ToString("O");
  var fp=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
  return new(fp,index.IndexFingerprint,stamp,index.Entries.ToArray());
 }
 public static void Validate(ResearchWorkspaceSnapshot snapshot)
 {
  var index=ResearchWorkspaceIndexRules.Create(snapshot.Entries);
  if(index.IndexFingerprint!=snapshot.IndexFingerprint || Create(index,snapshot.CreatedUtc).SnapshotFingerprint!=snapshot.SnapshotFingerprint)
   throw new InvalidOperationException("Workspace snapshot identity is invalid.");
 }
}
public sealed class ResearchWorkspaceSnapshotFileStore
{
 private readonly string _path; public ResearchWorkspaceSnapshotFileStore(string path)=>_path=Path.GetFullPath(path);
 public void Save(ResearchWorkspaceSnapshot value){ResearchWorkspaceSnapshotRules.Validate(value);Directory.CreateDirectory(Path.GetDirectoryName(_path)!);File.WriteAllText(_path,JsonSerializer.Serialize(value,new JsonSerializerOptions{WriteIndented=true}),new UTF8Encoding(false));}
 public ResearchWorkspaceSnapshot Load(){var value=JsonSerializer.Deserialize<ResearchWorkspaceSnapshot>(File.ReadAllText(_path))??throw new InvalidOperationException("Workspace snapshot is invalid.");ResearchWorkspaceSnapshotRules.Validate(value);return value;}
}
