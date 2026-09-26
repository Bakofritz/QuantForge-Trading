using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ResearchOptimizationFileStoreTests
{
    [Fact]
    public void Blocked_optimization_result_is_stored_and_recovered_without_a_selected_variant()
    {
        var report = new ResearchReport(
            "job-blocked", ResearchResultStatus.DataBlocked, "data", "strategy", "exec", "params", "partition",
            "blocked", null, null);
        var result = new ResearchOptimizationApplicationResult(
            new ResearchOptimizationResult(new[] { report }, false, "blocked"), null, "optimization-sha");
        var root = Path.Combine(Path.GetTempPath(), "qf-opt-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new ResearchOptimizationFileStore(root);
            var path = store.Save(result);
            Assert.True(File.Exists(path));
            Assert.Contains("selected-job=n/a", File.ReadAllText(path), StringComparison.Ordinal);

            var recovered = store.Load("optimization-sha");
            Assert.False(recovered.Complete);
            Assert.Null(recovered.SelectedJobId);
            Assert.False(recovered.LiveAccountEnabled);
            Assert.False(recovered.CanSubmitOrders);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void Recovery_rejects_tampered_text_representation()
    {
        var report = new ResearchReport(
            "job-blocked", ResearchResultStatus.DataBlocked, "data", "strategy", "exec", "params", "partition",
            "blocked", null, null);
        var result = new ResearchOptimizationApplicationResult(
            new ResearchOptimizationResult(new[] { report }, false, "blocked"), null, "optimization-sha");
        var root = Path.Combine(Path.GetTempPath(), "qf-opt-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new ResearchOptimizationFileStore(root);
            var path = store.Save(result);
            File.AppendAllText(path, "tampered\n");
            Assert.Throws<InvalidOperationException>(() => store.Load("optimization-sha"));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}
