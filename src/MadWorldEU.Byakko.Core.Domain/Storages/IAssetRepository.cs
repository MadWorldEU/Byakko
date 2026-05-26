namespace MadWorldEU.Byakko.Storages;

public interface IAssetRepository
{
    Task<Result> AddAsync(Asset asset);
    Task<Result> UpdateAsync(Asset asset);
    Task<Result<Asset>> FindAsync(Id id);
    Task<Result<List<Asset>>> GetExpiredAsync();
}