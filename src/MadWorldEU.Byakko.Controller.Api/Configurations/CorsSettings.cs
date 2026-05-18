namespace MadWorldEU.Byakko.Configurations;

/// <summary>CORS configuration bound from the <c>Cors</c> appsettings section.</summary>
internal sealed class CorsSettings
{
    internal const string Key = "Cors";

    /// <summary>Allowed origins. An empty array permits any origin.</summary>
    public string[] AllowedOrigins { get; init; } = [];
}