using MadWorldEU.Byakko.Accounts;
using MadWorldEU.Byakko.DomainDrivenDevelopment;

namespace MadWorldEU.Byakko.Storages;

/// <summary>Deletes all stored content and asset records for a user when their account has been permanently deleted.</summary>
public sealed class AssetAccountEventHandler(IAssetRepository assetRepository, IContentStorage contentStorage, ILogger<AssetAccountEventHandler> logger) : IDomainEventHandler<AccountDeletedEvent>
{
    /// <summary>Handles the <see cref="AccountDeletedEvent"/> by removing the user's asset content from storage and their asset records from the database.</summary>
    public async Task Handle(AccountDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var assetsResult = await assetRepository.GetAssetsAsync(domainEvent.UserId);
        if (assetsResult.IsFailure) return;

        foreach (var asset in assetsResult.Value)
        {
            var contentResult = await contentStorage.DeleteAsync(asset.GetPath());
            if (contentResult.IsFailure)
            {
                logger.LogWarning("Failed to delete asset {AssetId} for user {UserId}", asset.Id, domainEvent.UserId);
                continue;
            }
            
            logger.LogInformation("Deleted asset {AssetId} for user {UserId}", asset.Id, domainEvent.UserId);
        }
        
        await assetRepository.DeleteAsync(domainEvent.UserId);
    }
}