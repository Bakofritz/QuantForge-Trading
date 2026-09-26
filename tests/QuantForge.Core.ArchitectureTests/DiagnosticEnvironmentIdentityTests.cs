using QuantForge.Core;

namespace QuantForge.Core.ArchitectureTests;

public sealed class DiagnosticEnvironmentIdentityTests
{
    [Fact]
    public void Environment_CarriesRevisionVersionAndBuildCodeTogether()
    {
        var environment = new DiagnosticEnvironment(
            "0.30.31-automellon+abc123", "0.30.31", "3031", "SM-S938U", "Samsung", "16", 1440, 3120, 3m);

        Assert.Equal("0.30.31-automellon+abc123", environment.Build);
        Assert.Equal("0.30.31", environment.AppVersion);
        Assert.Equal("3031", environment.AppBuildCode);
    }
}
