namespace QuantForge.Core;

public static class CausalIntegrity
{
    public static void RequireInformationAvailableAtObservation(
        DateTimeOffset observationTime,
        DateTimeOffset informationTime)
    {
        if (informationTime > observationTime)
            throw new InvalidOperationException(
                "Information timestamp cannot be after observation timestamp.");
    }

    public static void RequireClosedBar(
        DateTimeOffset observationTime,
        DateTimeOffset barCloseTime)
    {
        if (barCloseTime > observationTime)
            throw new InvalidOperationException(
                "An unfinished bar cannot be used as observed information.");
    }
}
