namespace MadWorldEU.Byakko.Storages;

public sealed class UploadAssetContentUseCase(IAssetRepository assetRepository, IContentStorage contentStorage)
{
    public async Task<Result<UploadAssetContentResponse>> ExecuteAsync(string assetId, Stream content)
    {
        var id = Id.Create(assetId);
        if (id.IsFailure) return id.Error;

        var asset = await assetRepository.FindAsync(id.Value);
        if (asset.IsFailure) return asset.Error;

        var result = await contentStorage.UploadAsync(asset.Value.GetPath(), content);
        if (result.IsFailure) return result.Error;

        return new UploadAssetContentResponse()
        {
            Id = asset.Value.Id.Value
        };
    }
}