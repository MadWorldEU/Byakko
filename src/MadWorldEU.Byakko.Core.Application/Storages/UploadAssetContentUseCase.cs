namespace MadWorldEU.Byakko.Storages;

public sealed class UploadAssetContentUseCase(IClock clock, IAssetRepository assetRepository, IContentStorage contentStorage)
{
    public async Task<Result<UploadAssetContentResponse>> ExecuteAsync(string assetId, Stream content, long sizeInBytes, string userId, string fileName, string contentType)
    {
        var id = Id.Create(assetId);
        if (id.IsFailure) return id.Error;

        var sizeResult = Size.Create(sizeInBytes);
        if (sizeResult.IsFailure) return sizeResult.Error;

        var userIdResult = UserId.Create(userId);
        if (userIdResult.IsFailure) return userIdResult.Error;

        var asset = await assetRepository.FindAsync(id.Value);
        if (asset.IsFailure) return asset.Error;

        if (asset.Value.CreatedBy != userIdResult.Value)
        {
            return AssetErrors.Forbidden;
        }

        if (asset.Value.Name.Value != fileName)
        {
            return AssetErrors.FileNameMismatch;
        }

        if (!asset.Value.ContentType.Value.Equals(contentType, StringComparison.OrdinalIgnoreCase))
        {
            return AssetErrors.ContentTypeMismatch;
        }

        var uploadResult = await contentStorage.UploadAsync(asset.Value.GetPath(), content);
        if (uploadResult.IsFailure) return uploadResult.Error;

        var updateSizeResult = asset.Value.UpdateSize(clock, sizeResult.Value);
        if (updateSizeResult.IsFailure) return updateSizeResult.Error;

        var updateResult = await assetRepository.UpdateAsync(asset.Value);
        if (updateResult.IsFailure) return updateResult.Error;

        return new UploadAssetContentResponse { Id = asset.Value.Id.Value };
    }
}