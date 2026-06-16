namespace MadWorldEU.Byakko.Storages;

public sealed class GetStorageStatisticsUseCase(IAssetRepository assetRepository)
{
    public async Task<Result<GetStorageStatisticsResponse>> QueryAsync()
    {
        var savedSizeResult = await assetRepository.GetTotalSavedSizeAsync();
        if (savedSizeResult.IsFailure) return savedSizeResult.Error;
        
        var countAssetsResult = await assetRepository.GetCountOfActiveAssetsAsync();
        if (countAssetsResult.IsFailure) return countAssetsResult.Error;
        
        return new GetStorageStatisticsResponse()
        {
            TotalBytes = savedSizeResult.Value.Value,
            TotalFiles = countAssetsResult.Value
        };
    }
}