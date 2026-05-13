namespace MadWorldEU.Byakko.Storages;

/// <summary>Errors for the ContentType value object.</summary>
public static class ContentTypeErrors
{
    public static readonly Error Empty = Error.Create("ContentType.Empty", "Content type cannot be empty.");
    public static readonly Error TooLong = Error.Create("ContentType.TooLong", $"Content type cannot exceed {ContentType.MaxLength} characters.");
    public static readonly Error Invalid = Error.Create("ContentType.Invalid", "Content type is not a valid MIME type.");
}