namespace MadWorldEU.Byakko.Storages;

/// <summary>Errors for the ContentStorage infrastructure service.</summary>
internal static class ContentStorageErrors
{
    public static readonly Error UploadFailed = Error.Create("ContentStorage.UploadFailed", "Failed to upload content to storage.");
    public static readonly Error DownloadFailed = Error.Create("ContentStorage.DownloadFailed", "Failed to download content from storage.");
}