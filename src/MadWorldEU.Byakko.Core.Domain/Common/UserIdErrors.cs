namespace MadWorldEU.Byakko.Common;

/// <summary>Errors for the UserId value object.</summary>
public static class UserIdErrors
{
    public static readonly Error Empty = Error.Create("UserId.Empty", "User ID cannot be empty.");
    public static readonly Error Invalid = Error.Create("UserId.Invalid", "User ID is not a valid GUID.");
}