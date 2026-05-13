namespace MadWorldEU.Byakko.Storages;

/// <summary>Errors for the AssetPath value object.</summary>
public static class AssetPathErrors
{
    public static readonly Error PathEmpty = Error.Create("AssetPath.PathEmpty", "Asset path cannot be empty.");
    public static readonly Error PathTooLong = Error.Create("AssetPath.PathTooLong", $"Asset path cannot exceed {AssetPath.MaxLength} characters.");

    public static readonly Error KeyEmpty = Error.Create("AssetPath.KeyEmpty", "Asset key cannot be empty.");
    public static readonly Error KeyTooLong = Error.Create("AssetPath.KeyTooLong", $"Asset key cannot exceed {AssetPath.MaxLength} characters.");
}