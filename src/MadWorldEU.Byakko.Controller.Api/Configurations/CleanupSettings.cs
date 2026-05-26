using NodaTime;

namespace MadWorldEU.Byakko.Configurations;

/// <summary>Configuration for scheduled cleanup jobs, bound from the <c>Cleanup</c> appsettings section.</summary>
internal sealed class CleanupSettings
{
    internal const string Key = "Cleanup";

    /// <summary>UTC hour (0–23) at which the expired asset cleanup runs each day.</summary>
    public int TriggerHourUtc { get; init; } = 2;
    
    internal TimeSpan CalculateDelayUntilNextTrigger(IClock clock)
    {
        var now = clock.GetCurrentInstant();
        var nextTrigger = now.Plus(Duration.FromHours(TriggerHourUtc));

        if (nextTrigger <= now)
        {
            nextTrigger = nextTrigger.Plus(Duration.FromDays(1));
        }

        return (nextTrigger - now).ToTimeSpan();
    }
}