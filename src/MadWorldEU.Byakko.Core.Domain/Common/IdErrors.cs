namespace MadWorldEU.Byakko.Common;

/// <summary>Errors for the Id value object.</summary>
public static class IdErrors
{
    public static readonly Error Empty = Error.Create("Id.Empty", "Id cannot be empty.");
    public static readonly Error Invalid = Error.Create("Id.Invalid", "Id is not a valid GUID.");
}