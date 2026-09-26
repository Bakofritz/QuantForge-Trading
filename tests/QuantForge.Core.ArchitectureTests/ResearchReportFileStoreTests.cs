using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchReportFileStoreTests
{
    [Fact]
    public void Report_store_round_trips_terminal_report()
    {
        var root = Path.Combine(Path.GetTempPath(), "qf-report-" + Guid.NewGuid().ToString("N"));
        try
        {
            var report = new ResearchReport("job", ResearchResultStatus.DataBlocked, "dataset", "strategy", "timing", "params", "wf", "blocked", null, null);
            var store = new ResearchReportFileStore(root);
            store.Save(report);
            var loaded = store.Load("job");
            Assert.Equal(report, loaded);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}
