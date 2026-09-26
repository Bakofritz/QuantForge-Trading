using QuantForge.Core;
using Xunit;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchLaunchIntentFileStoreTests
{
    [Fact]
    public void Launch_package_round_trips_with_gate_validation()
    {
        var gates = TestFixtures.Gates();
        var intent = ResearchLaunchIntentRules.Create(gates, TestFixtures.Job(gates));
        var package = new ResearchLaunchPackage(gates, intent);
        var root = Path.Combine(Path.GetTempPath(), "qf-launch-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new ResearchLaunchIntentFileStore(root);
            store.Save(package);
            Assert.Equal(package, store.Load(intent.IntentFingerprint));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}
