using MadWorldEU.Byakko.Functional;

namespace MadWorldEU.Byakko.Storages;

public sealed class AssetRepository(ByakkoContext context) : IAssetRepository
{
    public async Task<Result> AddAsync(Asset asset)
    {
        await context.Assets.AddAsync(asset);
        await context.SaveChangesAsync();

        return Result.Success();
    }
}