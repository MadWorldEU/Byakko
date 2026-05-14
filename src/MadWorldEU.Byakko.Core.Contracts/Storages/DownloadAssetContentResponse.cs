namespace MadWorldEU.Byakko.Storages;

/// <summary>Response containing the downloaded asset content stream and its metadata.</summary>
public sealed class DownloadAssetContentResponse
{
    public required Stream Content { get; init; }
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
}