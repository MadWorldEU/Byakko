using MadWorldEU.Byakko.Audits;
using MadWorldEU.Byakko.DomainDrivenDevelopment;

namespace MadWorldEU.Byakko.Storages;

/// <summary>Deletes the content of an asset owned by the requesting user.</summary>
public sealed class DeleteMyAssetContentUseCase(
    IClock clock,
    IAssetRepository repository,
    IContentStorage contentStorage,
    IDomainEventsDispatcher domainEventsDispatcher,
    IAssetMetrics metrics,
    ILogger<DeleteMyAssetContentUseCase> logger)
{
    /// <summary>Executes the use case. Returns <see cref="AssetErrors.Forbidden"/> when the asset does not belong to the user.</summary>
    public async Task<Result<DeleteAssetContentResponse>> ExecuteAsync(string assetId, string userId, System.Net.IPAddress? ipAddress)
    {
        var ipAddressResult = IpAddress.Create(ipAddress);
        if (ipAddressResult.IsFailure) return ipAddressResult.Error;

        var id = Id.Create(assetId);
        if (id.IsFailure) return id.Error;

        var userIdResult = UserId.Create(userId);
        if (userIdResult.IsFailure) return userIdResult.Error;

        var assetResult = await repository.FindAsync(id.Value);
        if (assetResult.IsFailure) return assetResult.Error;

        var asset = assetResult.Value;

        if (asset.CreatedBy != userIdResult.Value)
        {
            return AssetErrors.Forbidden;
        }

        var deleteResult = await contentStorage.DeleteAsync(asset.GetPath());
        if (deleteResult.IsFailure)
        {
            logger.LogError("Failed to delete asset content: {Error}", deleteResult.Error.Description);
            return deleteResult.Error;
        }

        var softDeleteResult = asset.Delete(clock);
        if (softDeleteResult.IsFailure)
        {
            logger.LogError("Failed to soft-delete asset '{AssetId}': {Error}", asset.Id.Value, softDeleteResult.Error.Description);
            return softDeleteResult.Error;
        }

        var updateResult = await repository.UpdateAsync(asset);
        if (updateResult.IsFailure)
        {
            logger.LogError("Failed to mark asset '{AssetId}' as deleted: {Error}", asset.Id.Value, updateResult.Error.Description);
            return updateResult.Error;
        }

        var assetContentDeletedEvent = new AssetContentDeletedEvent(asset.Id, ipAddressResult.Value, asset.CreatedBy, asset.CreatedAt);
        await domainEventsDispatcher.DispatchAsync([assetContentDeletedEvent]);
        
        metrics.RecordContentDeleted();
        return new DeleteAssetContentResponse
        {
            Id = asset.Id.Value
        };
    }
}