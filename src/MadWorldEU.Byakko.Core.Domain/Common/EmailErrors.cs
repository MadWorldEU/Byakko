namespace MadWorldEU.Byakko.Common;

/// <summary>Errors for the Email value object.</summary>
public static class EmailErrors
{
    public static readonly Error Empty = Error.Create("Email.Empty", "Email address cannot be empty.");
    public static readonly Error Invalid = Error.Create("Email.Invalid", "Email address is not valid.");
}