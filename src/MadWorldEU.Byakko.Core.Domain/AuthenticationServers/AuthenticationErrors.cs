namespace MadWorldEU.Byakko.AuthenticationServers;

/// <summary>Domain errors for the authentication server.</summary>
public static class AuthenticationErrors
{
    public static readonly Error DeleteFailed = Error.Create("Authentication.DeleteFailed", "The user could not be deleted from the authentication server.");
    public static readonly Error ServerUnavailable = Error.Create("Authentication.ServerUnavailable", "The authentication server is unavailable.");
}