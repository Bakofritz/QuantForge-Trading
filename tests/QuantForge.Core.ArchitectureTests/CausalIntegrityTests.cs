using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public class CausalIntegrityTests
{
    [Fact]
    public void Future_information_is_rejected()
    {
        var observation = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var future = observation.AddMinutes(1);

        Assert.Throws<InvalidOperationException>(
            () => CausalIntegrity.RequireInformationAvailableAtObservation(observation, future));
    }

    [Fact]
    public void Unfinished_bar_is_rejected()
    {
        var observation = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var close = observation.AddSeconds(30);

        Assert.Throws<InvalidOperationException>(
            () => CausalIntegrity.RequireClosedBar(observation, close));
    }
}
