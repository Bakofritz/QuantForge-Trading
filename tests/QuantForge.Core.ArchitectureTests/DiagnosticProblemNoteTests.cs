using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class DiagnosticProblemNoteTests
{
    [Fact]
    public void Compose_PreservesExpectedAndActualAsStructuredBoundedText()
    {
        var text = DiagnosticProblemNote.Compose("Batch should complete", "Batch stopped after ZIP member 3");
        Assert.Equal("Expected: Batch should complete\nActual: Batch stopped after ZIP member 3", text);
        Assert.True(text.Length <= 500);
    }

    [Fact]
    public void Compose_RequiresAtLeastOneField()
    {
        Assert.Throws<ArgumentException>(() => DiagnosticProblemNote.Compose(" ", null));
    }
}
