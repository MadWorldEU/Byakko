namespace MadWorldEU.Byakko.Common;

/// <summary>Errors for the Name value object.</summary>
public static class NameErrors
{
    public static readonly Error Empty = Error.Create("Name.Empty", "Name cannot be empty.");
    public static readonly Error TooLong = Error.Create("Name.TooLong", $"Name cannot exceed {Name.MaxLength} characters.");
}