namespace MadWorldEU.Byakko.Storages;

/// <summary>Downloads the binary content of an asset from object storage.</summary>
public sealed class DownloadAssetContentUseCase(IAssetRepository assetRepository, IContentStorage contentStorage)
{
    public async Task<Result<DownloadAssetContentResponse>> ExecuteAsync(string assetId)
    {
        var id = Id.Create(assetId);
        if (id.IsFailure) return id.Error;

        var asset = await assetRepository.FindAsync(id.Value);
        if (asset.IsFailure) return asset.Error;

        var content = await contentStorage.DownloadAsync(asset.Value.GetPath());
        if (content.IsFailure) return content.Error;

        return new DownloadAssetContentResponse
        {
            Content = content.Value,
            ContentType = asset.Value.ContentType.Value,
            FileName = asset.Value.Name.Value
        };
    }
}