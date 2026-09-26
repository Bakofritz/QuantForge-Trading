using System.Security.Cryptography;
using System.Text;
namespace QuantForge.Core;
public sealed record ResearchReadinessSnapshot(string Fingerprint,string DatasetFingerprint,string StrategyFingerprint,bool DatasetAdmitted,bool SessionComplete,bool ReliabilityAdmissible,bool StrategyAdmitted,bool Ready);
public static class ResearchReadinessSnapshotRules
{
 public static ResearchReadinessSnapshot Create(ResearchGateBundle gates)
 {
  ResearchGateBundleRules.Validate(gates);
  var dataset=true; var session=gates.Coverage.Report.ResearchAdmissible;
  var reliability=DataReliabilityRules.IsResearchAdmissible(gates.Reliability); var strategy=gates.Strategy.Envelope.State==StrategyAdmissionState.Admitted;
  var text=string.Join("|",gates.Dataset.DatasetFingerprint,gates.Strategy.Envelope.Manifest.SourceFingerprint,dataset,session,reliability,strategy);
  var fp=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
  return new(fp,gates.Dataset.DatasetFingerprint,gates.Strategy.Envelope.Manifest.SourceFingerprint,dataset,session,reliability,strategy,dataset&&session&&reliability&&strategy);
 }
}
