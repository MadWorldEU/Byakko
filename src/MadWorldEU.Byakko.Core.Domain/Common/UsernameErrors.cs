namespace MadWorldEU.Byakko.Common;

/// <summary>Errors for the <see cref="Username"/> value object.</summary>
public static class UsernameErrors
{
    /// <summary>Returned when the username is null or whitespace.</summary>
    public static readonly Error Empty = Error.Create("Username.Empty", "Username cannot be empty.");
}