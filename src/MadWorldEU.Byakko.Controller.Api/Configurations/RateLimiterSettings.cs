namespace MadWorldEU.Byakko.Configurations;

/// <summary>Rate limiting configuration bound from the <c>RateLimiting</c> appsettings section.</summary>
internal sealed class RateLimiterSettings
{
    internal const string Key = "RateLimiting";

    public bool Enabled { get; init; } = true;
    public PolicySettings General { get; init; } = new();
    public PolicySettings Content { get; init; } = new();

    /// <summary>Settings for a single fixed-window rate limiting policy.</summary>
    internal sealed class PolicySettings
    {
        public int PermitLimit { get; init; }
        public int WindowInSeconds { get; init; }
    }
}