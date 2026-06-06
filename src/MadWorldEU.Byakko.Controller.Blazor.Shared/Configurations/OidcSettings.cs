namespace MadWorldEU.Byakko.Configurations;

/// <summary>OIDC configuration for Blazor clients, bound from the <c>Oidc</c> appsettings section.</summary>
public sealed class OidcSettings
{
    public const string Key = "Oidc";

    /// <summary>The Keycloak realm authority URL (e.g. https://auth.example.com/realms/myrealm).</summary>
    public string Authority { get; init; } = string.Empty;

    /// <summary>Returns the Keycloak account management URL for editing the user profile.</summary>
    public string GetEditAccountUrl() => $"{Authority.TrimEnd('/')}/account";
}