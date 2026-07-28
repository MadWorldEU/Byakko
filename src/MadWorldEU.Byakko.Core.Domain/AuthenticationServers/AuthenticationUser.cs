namespace MadWorldEU.Byakko.AuthenticationServers;

/// <summary>Represents a user account in the authentication server.</summary>
public class AuthenticationUser : Entity<UserId>
{
    /// <summary>The username associated with this authentication user.</summary>
    public Username Username { get; private init; }

    private AuthenticationUser(UserId id, Username username)
    {
        Id = id;
        Username = username;
    }

    /// <summary>Creates an <see cref="AuthenticationUser"/> from already-validated value objects.</summary>
    public static Result<AuthenticationUser> Create(UserId userId, Username username)
    {
        return new AuthenticationUser(userId, username);
    }

    /// <summary>Creates an <see cref="AuthenticationUser"/> from raw strings, validating the username. Returns a failure if validation fails.</summary>
    public static Result<AuthenticationUser> Create(UserId userId, string? username)
    {
        var usernameResult = Username.Create(username);

        return usernameResult.IsFailure
            ? Result.Failure<AuthenticationUser>(usernameResult.Error)
            : Create(userId, usernameResult.Value);
    }
}