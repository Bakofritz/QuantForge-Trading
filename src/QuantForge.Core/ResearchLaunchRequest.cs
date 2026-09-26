using System.Security.Cryptography;
using System.Text;
namespace QuantForge.Core;
public sealed record ResearchLaunchRequest(string RequestFingerprint,string ReadinessFingerprint,ResearchLaunchIntent Launch);
public static class ResearchLaunchRequestRules
{
 public static ResearchLaunchRequest Create(ResearchReadinessSnapshot readiness,ResearchLaunchIntent launch)
 {
  if(!readiness.Ready) throw new InvalidOperationException("Research launch requires complete readiness evidence.");
  if(readiness.DatasetFingerprint!=launch.Job.Identity.DatasetFingerprint||readiness.StrategyFingerprint!=launch.Job.Identity.StrategyFingerprint)throw new InvalidOperationException("Readiness and launch identities do not match.");
  var fp=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(readiness.Fingerprint+"|"+launch.IntentFingerprint)));
  return new(fp,readiness.Fingerprint,launch);
 }
}
