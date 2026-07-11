namespace MadWorldEU.Byakko.AuthenticationServers;

/// <summary>Repository for managing users in the authentication server.</summary>
public interface IAuthenticationRepository
{
    /// <summary>Permanently deletes the user account from the authentication server.</summary>
    Task<Result> DeleteUser(UserId userId);
}