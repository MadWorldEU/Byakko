using MadWorldEU.Byakko.Common.Pages;

namespace MadWorldEU.Byakko.Storages;

/// <summary>Persistence contract for <see cref="Asset"/> aggregates.</summary>
public interface IAssetRepository
{
    /// <summary>Persists a new asset to the store.</summary>
    Task<Result> AddAsync(Asset asset);

    /// <summary>Saves changes to an existing asset.</summary>
    Task<Result> UpdateAsync(Asset asset);
    
    /// <summary>Permanently deletes the given asset record from the store.</summary>
    Task<Result> DeleteAsync(Asset asset);

    /// <summary>Returns the asset with the given <paramref name="id"/>, or <c>Asset.NotFound</c> if absent.</summary>
    Task<Result<Asset>> FindAsync(Id id);
    
    /// <summary>Returns all assets owned by the given user.</summary>
    Task<Result<List<Asset>>> GetAssetsAsync(UserId userId);
    
    /// <summary>Returns a paged list of assets, optionally filtered by asset id or owner.</summary>
    Task<Result<PagedResult<Asset>>> GetAssetsAsync(Id id, UserId userId, Page page);

    /// <summary>Returns the total number of non-deleted assets across all users.</summary>
    Task<Result<int>> GetCountOfActiveAssetsAsync();

    /// <summary>Returns the number of non-deleted assets owned by <paramref name="userId"/>.</summary>
    Task<Result<int>> GetCountOfActiveAssetsAsync(UserId userId);

    /// <summary>Returns all assets whose expiry has passed but whose content has not yet been deleted.</summary>
    Task<Result<IReadOnlyList<Asset>>> GetExpiredContentAsync();

    /// <summary>Returns the sum of sizes of all non-deleted assets.</summary>
    Task<Result<Size>> GetTotalSavedSizeAsync();

    /// <summary>Hard-deletes asset records that were soft-deleted more than one year ago.</summary>
    Task<Result> DeleteExpiredAssets();
}