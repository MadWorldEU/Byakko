namespace MadWorldEU.Byakko.Configurations;

/// <summary>
/// URLs of external services to probe for health status.
/// </summary>
internal sealed class HealthCheckSettings
{
    public string Api { get; set; } = string.Empty;
    public string Admin { get; set; } = string.Empty;
    public string Portal { get; set; } = string.Empty;
}