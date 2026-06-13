namespace MadWorldEU.Byakko.Storages;

/// <summary>Errors for the Asset entity.</summary>
public static class AssetErrors
{
    public static readonly Error NotFound = Error.Create("Asset.NotFound", "Asset was not found.");
    public static readonly Error SaveFailed = Error.Create("Asset.SaveFailed", "Failed to save asset.");
    public static readonly Error QueryFailed = Error.Create("Asset.QueryFailed", "Failed to query asset.");
    public static readonly Error UpdateFailed = Error.Create("Asset.UpdateFailed", "Failed to update asset.");
    public static readonly Error Forbidden = Error.Create("Asset.Forbidden", "You are not allowed to modify this asset.");
    public static readonly Error FileNameMismatch = Error.Create("Asset.FileNameMismatch", "File name does not match the asset metadata.");
    public static readonly Error ContentTypeMismatch = Error.Create("Asset.ContentTypeMismatch", "Content type does not match the asset metadata.");
    public static readonly Error DeleteFailed = Error.Create("Asset.DeleteFailed", "Failed to delete asset.");
    public static readonly Error AlreadyDeleted = Error.Create("Asset.AlreadyDeleted", "Asset has already been deleted.");
    public static readonly Error SizeAlreadySet = Error.Create("Asset.SizeAlreadySet", "Asset size has already been set.");
    public static readonly Error Expired = Error.Create("Asset.Expired", "Asset has expired and is no longer available for download.");
    public static readonly Error MaxFilesReached = Error.Create("Asset.MaxFilesReached", "You have reached the maximum number of active files allowed.");
    public static readonly Error FileTooLarge = Error.Create("Asset.FileTooLarge", "The file exceeds the maximum allowed upload size.");
}