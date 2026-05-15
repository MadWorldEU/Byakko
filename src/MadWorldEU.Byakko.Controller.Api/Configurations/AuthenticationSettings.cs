namespace MadWorldEU.Byakko.Configurations;

/// <summary>Configuration options for JWT Bearer authentication, bound from the <c>Authentication</c> section in appsettings.</summary>
internal sealed class AuthenticationSettings
{
    internal const string Key = "Authentication";

    /// <summary>Keycloak realm URL used as the JWT authority (e.g. <c>http://localhost:4321/realms/byakko</c>).</summary>
    internal string Authority { get; init; } = string.Empty;

    /// <summary>Expected audience claim in the JWT, matching the Keycloak client ID.</summary>
    internal string Audience { get; init; } = string.Empty;

    /// <summary>When <c>false</c>, signature and claim validation is skipped — for local development only.</summary>
    internal bool ValidateUser { get; init; }
}