using System.Text;
using System.Text.Json;
namespace QuantForge.Core;
public sealed class ResearchResumeCheckpointFileStore
{
 private readonly string _path; public ResearchResumeCheckpointFileStore(string path)=>_path=Path.GetFullPath(path);
 public void Save(ResearchResumeCheckpoint value){if(string.IsNullOrWhiteSpace(value.CheckpointFingerprint))throw new InvalidOperationException("Resume checkpoint fingerprint is required.");Directory.CreateDirectory(Path.GetDirectoryName(_path)!);File.WriteAllText(_path,JsonSerializer.Serialize(value,new JsonSerializerOptions{WriteIndented=true}),new UTF8Encoding(false));}
 public ResearchResumeCheckpoint Load(){var value=JsonSerializer.Deserialize<ResearchResumeCheckpoint>(File.ReadAllText(_path))??throw new InvalidOperationException("Resume checkpoint JSON is invalid.");var expected=ResearchResumeCheckpointRules.Create(value.WorkspaceFingerprint,value.Stage,value.LaunchFingerprint,value.OutcomeFingerprint);if(expected!=value)throw new InvalidOperationException("Resume checkpoint fingerprint is invalid.");return value;}
}
