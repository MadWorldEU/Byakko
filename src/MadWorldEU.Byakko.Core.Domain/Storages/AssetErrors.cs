namespace MadWorldEU.Byakko.Storages;

/// <summary>Errors for the Asset entity.</summary>
public static class AssetErrors
{
    public static readonly Error NotFound = Error.Create("Asset.NotFound", "Asset was not found.");
    public static readonly Error SaveFailed = Error.Create("Asset.SaveFailed", "Failed to save asset.");
    public static readonly Error QueryFailed = Error.Create("Asset.QueryFailed", "Failed to query asset.");
}