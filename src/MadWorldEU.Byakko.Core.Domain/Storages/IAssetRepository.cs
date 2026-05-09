namespace MadWorldEU.Byakko.Storages;

public interface IAssetRepository
{
    Task<Result> AddAsync(Asset asset);
}