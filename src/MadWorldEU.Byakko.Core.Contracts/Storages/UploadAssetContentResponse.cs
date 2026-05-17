namespace MadWorldEU.Byakko.Storages;

/// <summary>Response returned after successfully uploading binary content for an asset.</summary>
public sealed class UploadAssetContentResponse
{
    public required Guid Id { get; init; }
}