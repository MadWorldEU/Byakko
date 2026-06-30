namespace MadWorldEU.Byakko.Storages;

public sealed class DeleteAllExpiredContentOfAssetsUseCase(
    IClock clock, 
    IAssetRepository repository, 
    IContentStorage contentStorage, 
    IAssetMetrics metrics,
    ILogger<DeleteAllExpiredContentOfAssetsUseCase> logger)
{
    public async Task<Result> ExecuteAsync()
    {
        var expiredAssetsResult = await repository.GetExpiredContentAsync();

        if (expiredAssetsResult.IsFailure)
        {
            logger.LogError("Failed to retrieve expired assets: {Error}", expiredAssetsResult.Error.Description);
            return Result.Failure(expiredAssetsResult.Error);
        }

        var expiredAssets = expiredAssetsResult.Value;
        logger.LogInformation("Found {Count} expired asset(s) to process.", expiredAssets.Count);

        foreach (var asset in expiredAssets)
        {
            var deleteResult = await contentStorage.DeleteAsync(asset.GetPath());
            if (deleteResult.IsFailure)
            {
                logger.LogError("Failed to delete content for asset '{AssetId}': {Error}", asset.Id.Value, deleteResult.Error.Description);
                continue;
            }

            var softDeleteResult = asset.Delete(clock);
            if (softDeleteResult.IsFailure)
            {
                logger.LogError("Failed to soft-delete asset '{AssetId}': {Error}", asset.Id.Value, softDeleteResult.Error.Description);
                continue;
            }

            var updateResult = await repository.UpdateAsync(asset);
            if (updateResult.IsFailure)
            {
                logger.LogError("Failed to mark asset '{AssetId}' as deleted: {Error}", asset.Id.Value, updateResult.Error.Description);
                continue;
            }

            metrics.RecordContentDeleted();
            logger.LogInformation("Asset '{AssetId}' content deleted and marked as deleted.", asset.Id.Value);
        }

        return Result.Success();
    }
}