using Keycloak.AuthServices.Sdk.Admin;
using MadWorldEU.Byakko.Configurations;
using Microsoft.Extensions.Options;

namespace MadWorldEU.Byakko.AuthenticationServers;

/// <summary>Keycloak implementation of <see cref="IAuthenticationRepository"/>.</summary>
internal sealed class AuthenticationRepository(
    IKeycloakClient keycloakClient,
    IOptions<KeyCloakSettings> settings,
    ILogger<AuthenticationRepository> logger) : IAuthenticationRepository
{
    private readonly string _managedRealm = settings.Value.ManagedRealm;

    /// <summary>Permanently deletes the user from the Keycloak realm.</summary>
    public async Task<Result> DeleteUser(UserId userId)
    {
        try
        {
            var realm = await keycloakClient.GetRealmAsync(_managedRealm);

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

            await keycloakClient.DeleteUserAsync(_managedRealm, userId.Value.ToString());
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