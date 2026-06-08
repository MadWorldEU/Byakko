using MadWorldEU.Byakko.Common.Pages;

namespace MadWorldEU.Byakko.Storages;

public interface IAssetRepository
{
    Task<Result> AddAsync(Asset asset);
    Task<Result> UpdateAsync(Asset asset);
    Task<Result<Asset>> FindAsync(Id id);
    Task<Result<PagedResult<Asset>>> GetAllPagesAsync(Id id, UserId userId, Page page);
    Task<Result<int>> GetCountOfActiveAssetsAsync();
    Task<Result<IReadOnlyList<Asset>>> GetExpiredContentAsync();
    Task<Result<Size>> GetTotalSavedSizeAsync();
    Task<Result> DeleteExpiredAssets();
}