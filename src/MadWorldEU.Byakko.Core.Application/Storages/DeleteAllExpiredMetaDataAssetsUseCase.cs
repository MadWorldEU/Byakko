namespace MadWorldEU.Byakko.Storages;

public sealed class DeleteAllExpiredMetaDataAssetsUseCase(IAssetRepository repository, IAssetMetrics metrics, ILogger<DeleteAllExpiredMetaDataAssetsUseCase> logger)
{
    public async Task<Result> ExecuteAsync()
    {
        logger.LogInformation("Starting expired asset metadata cleanup.");

        var result = await repository.DeleteExpiredAssets();

        if (result.IsFailure)
        {
            logger.LogError("Failed to delete expired asset metadata: {Error}", result.Error.Description);
            return result;
        }

        metrics.RecordMetadataDeleted();
        logger.LogInformation("Expired asset metadata cleanup completed.");
        return result;
    }
}