namespace MadWorldEU.Byakko.Configurations;

/// <summary>Configuration for the Keycloak admin client, bound from the <c>KeyCloak</c> appsettings section.</summary>
internal sealed class KeyCloakSettings
{
    public const string Key = "KeyCloak";

    /// <summary>Keycloak server base URL (e.g. https://authentication.byakko.dev/).</summary>
    public string AuthServerUrl { get; init; } = string.Empty;

    /// <summary>Client ID of the admin service account registered in the master realm.</summary>
    public string Resource { get; init; } = string.Empty;

    /// <summary>Client secret for the admin service account. Never commit a real value — override via environment variable or Kubernetes secret.</summary>
    public string AdminClientSecret { get; init; } = string.Empty;

    /// <summary>Name of the Keycloak realm whose users are managed (e.g. MadWorld).</summary>
    public string ManagedRealm { get; init; } = string.Empty;
}