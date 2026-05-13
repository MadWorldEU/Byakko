using MadWorldEU.Byakko.Functional;
using Microsoft.Extensions.Logging;

namespace MadWorldEU.Byakko.Storages;

public sealed class AssetRepository(ByakkoContext context, ILogger<AssetRepository> logger) : IAssetRepository
{
    public async Task<Result> AddAsync(Asset asset)
    {
        try
        {
            await context.Assets.AddAsync(asset);
            await context.SaveChangesAsync();

            logger.LogInformation("Asset '{AssetId}' added successfully.", asset.Id.Value);
            return Result.Success();
        }
        catch (DbUpdateException exception)
        {
            logger.LogError(exception, "Failed to save asset '{AssetId}'.", asset.Id.Value);
            return Result.Failure(AssetErrors.SaveFailed);
        }
    }

    public async Task<Result<Asset>> FindAsync(Id id)
    {
        Asset? asset;

        try
        {
            asset = await context.Assets.FirstOrDefaultAsync(a => a.Id == id);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to query asset '{AssetId}'.", id.Value);
            return AssetErrors.QueryFailed;
        }

        if (asset is null)
        {
            logger.LogInformation("Asset with id '{AssetId}' not found.", id.Value);
            return AssetErrors.NotFound;
        }

        return asset;
    }
}