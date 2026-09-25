using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class ProductUiDataReliabilityPresenterTests
{
    [Fact]
    public void CleanBenchmarkedDataset_IsPresentedAsAdmissible()
    {
        var assessment = new DataReliabilityAssessment(
            "dataset-a", 99m, true, 0, 0, null);

        var state = ProductUiDataReliabilityPresenter.Create(assessment);

        Assert.Equal(ProductUiDataReliabilityStatus.Admissible, state.Status);
        Assert.Equal(99m, state.ScorePercent);
        Assert.True(state.ComparedToLiveBenchmark);
    }

    [Fact]
    public void DatasetWithGap_RemainsExplicitlyBlocked()
    {
        var assessment = new DataReliabilityAssessment(
            "dataset-a", 90m, true, 1, 0, "One unresolved gap remains.");

        var state = ProductUiDataReliabilityPresenter.Create(assessment);

        Assert.Equal(ProductUiDataReliabilityStatus.Blocked, state.Status);
        Assert.Equal(1, state.UnresolvedGapCount);
        Assert.Equal("One unresolved gap remains.", state.Limitation);
    }

    [Fact]
    public void DatasetWithoutLiveBenchmark_RemainsExplicitlyBlocked()
    {
        var assessment = new DataReliabilityAssessment(
            "dataset-a", 88m, false, 0, 0, "Live benchmark comparison has not been completed.");

        var state = ProductUiDataReliabilityPresenter.Create(assessment);

        Assert.Equal(ProductUiDataReliabilityStatus.Blocked, state.Status);
        Assert.False(state.ComparedToLiveBenchmark);
        Assert.False(string.IsNullOrWhiteSpace(state.Limitation));
    }
}
