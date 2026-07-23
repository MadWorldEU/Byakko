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

    public async Task<Result> FindUser(UserId userId)
    {
        try
        {
            await keycloakClient.GetUserAsync(_managedRealm, userId.Value.ToString());
            logger.LogInformation("User '{UserId}' found at the authentication server.", userId.Value);

            return Result.Success();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to find user '{UserId}' at the authentication server.", userId.Value);
            return Result.Failure(AuthenticationErrors.ServerUnavailable);
        }
    }
    
    /// <summary>Permanently deletes the user from the Keycloak realm.</summary>
    public async Task<Result> DeleteUser(UserId userId)
    {
        try
        {
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