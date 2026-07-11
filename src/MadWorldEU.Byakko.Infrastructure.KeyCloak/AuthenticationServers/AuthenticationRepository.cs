using Keycloak.AuthServices.Sdk.Admin;

namespace MadWorldEU.Byakko.AuthenticationServers;

/// <summary>Keycloak implementation of <see cref="IAuthenticationRepository"/>.</summary>
public sealed class AuthenticationRepository(
    IKeycloakRealmClient keycloakRealmClient,
    ILogger<AuthenticationRepository> logger) : IAuthenticationRepository
{
    /// <summary>Permanently deletes the user from the Keycloak realm.</summary>
    public async Task<Result> DeleteUser(UserId userId)
    {
        try
        {
            var realm = await keycloakRealmClient.GetRealmAsync("");

            if (realm.Users is null)
            {
                logger.LogError("Realm users not found");
                return Result.Failure(AuthenticationErrors.RealmUsersNotFound);
            }

            var user = realm.Users.FirstOrDefault(u => u.Id == userId.Value.ToString());

            if (user is null)
            {
                logger.LogWarning("User '{UserId}' not found in authentication server.", userId.Value);
                return Result.Failure(AuthenticationErrors.UserNotFound);
            }

            realm.Users.Remove(user);
            logger.LogInformation("User '{UserId}' deleted from authentication server.", userId.Value);

            return Result.Success();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to delete user '{UserId}' from authentication server.", userId.Value);
            return Result.Failure(AuthenticationErrors.ServerUnavailable);
        }
    }
}