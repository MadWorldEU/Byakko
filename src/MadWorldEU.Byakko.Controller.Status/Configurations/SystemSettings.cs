namespace MadWorldEU.Byakko.Configurations;

/// <summary>
/// Deployment metadata injected at build time via appsettings.
/// </summary>
public sealed class SystemSettings
{
    public string Tag { get; init; } = string.Empty;
    public GitSettings Git { get; init; } = new();
}

/// <summary>
/// Source control metadata injected at build time.
/// </summary>
public sealed class GitSettings
{
    public string Repository { get; init; } = string.Empty;
}