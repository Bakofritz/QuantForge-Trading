using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchReportFileStoreIntegrityTests
{
    [Fact]
    public void Tampered_markdown_is_rejected_on_recovery()
    {
        var root = Path.Combine(Path.GetTempPath(), "qf-report-integrity-" + Guid.NewGuid().ToString("N"));
        try
        {
            var report = new ResearchReport("job", ResearchResultStatus.DataBlocked, "dataset", "strategy", "timing", "params", "wf", "blocked", null, null);
            var store = new ResearchReportFileStore(root);
            store.Save(report);
            File.AppendAllText(Path.Combine(root, "job", "report.md"), "tamper");
            Assert.Throws<InvalidOperationException>(() => store.Load("job"));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}
