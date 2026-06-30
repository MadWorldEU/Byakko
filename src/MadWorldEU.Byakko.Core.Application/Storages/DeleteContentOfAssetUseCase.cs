using MadWorldEU.Byakko.Audits;
using MadWorldEU.Byakko.DomainDrivenDevelopment;

namespace MadWorldEU.Byakko.Storages;

public sealed class DeleteContentOfAssetUseCase(
    IClock clock,
    IAssetRepository repository,
    IContentStorage contentStorage,
    IDomainEventsDispatcher domainEventsDispatcher,
    IAssetMetrics metrics,
    ILogger<DeleteContentOfAssetUseCase> logger)
{
    public async Task<Result<DeleteAssetContentResponse>> ExecuteAsync(string assetId, System.Net.IPAddress? ipAddress)
    {
        var ipAddressResult = IpAddress.Create(ipAddress);
        if (ipAddressResult.IsFailure) return ipAddressResult.Error;
        
        var id = Id.Create(assetId);
        if (id.IsFailure) return id.Error;
        
        var assetResult = await repository.FindAsync(id.Value);
        if (assetResult.IsFailure) return assetResult.Error;
        
        var asset = assetResult.Value;
        var deleteResult = await contentStorage.DeleteAsync(asset.GetPath());
        if (deleteResult.IsFailure)
        {
            logger.LogError("Failed to delete expired asset metadata: {Error}", deleteResult.Error.Description);
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
        
        var assetMetaDataCreatedEvent = new AssetContentDeletedEvent(assetResult.Value.Id, ipAddressResult.Value, assetResult.Value.CreatedBy, assetResult.Value.CreatedAt);
        await domainEventsDispatcher.DispatchAsync([assetMetaDataCreatedEvent]);

        metrics.RecordContentDeleted();
        return new DeleteAssetContentResponse
        {
            Id = assetResult.Value.Id.Value
        };
    }
}