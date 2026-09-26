using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class WalkForwardResearchFileStoreTests
{
    [Fact]
    public void Store_round_trips_verified_walk_forward_result()
    {
        var result = WalkForwardResearchRunner.Run(WalkForwardResearchPlanRules.Create(new[]
        {
            WalkForwardResearchPlanTests.FixtureSegment()
        }));
        var root = Path.Combine(Path.GetTempPath(), "qf-wf-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new WalkForwardResearchFileStore(root);
            var path = store.Save(result);
            Assert.True(File.Exists(path));
            var recovered = store.Load(result.ResultFingerprint);
            Assert.Equal(result.ResultFingerprint, recovered.ResultFingerprint);
            Assert.True(recovered.Complete);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void Store_rejects_tampered_markdown_representation()
    {
        var result = WalkForwardResearchRunner.Run(WalkForwardResearchPlanRules.Create(new[]
        {
            WalkForwardResearchPlanTests.FixtureSegment()
        }));
        var root = Path.Combine(Path.GetTempPath(), "qf-wf-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new WalkForwardResearchFileStore(root);
            var jsonPath = store.Save(result);
            var markdownPath = Path.Combine(Path.GetDirectoryName(jsonPath)!, "walk-forward.md");
            File.AppendAllText(markdownPath, "tampered\n");
            Assert.Throws<InvalidOperationException>(() => store.Load(result.ResultFingerprint));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}
