namespace MadWorldEU.Byakko.AuthenticationServers;

/// <summary>Domain errors for the authentication server.</summary>
public static class AuthenticationErrors
{
    public static readonly Error RealmUsersNotFound = Error.Create("Authentication.RealmUsersNotFound", "The realm users could not be found.");
    public static readonly Error UserNotFound = Error.Create("Authentication.UserNotFound", "The user could not be found in the authentication server.");
    public static readonly Error DeleteFailed = Error.Create("Authentication.DeleteFailed", "The user could not be deleted from the authentication server.");
    public static readonly Error ServerUnavailable = Error.Create("Authentication.ServerUnavailable", "The authentication server is unavailable.");
}