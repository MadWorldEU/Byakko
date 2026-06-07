namespace MadWorldEU.Byakko.Storages;

/// <summary>Returns a paged list of asset metadata records.</summary>
public sealed class GetAssetsMetaDataUseCase(IAssetRepository assetRepository)
{
    public async Task<Result<GetAssetsMetadataResponse>> ExecuteAsync(int page)
    {
        var pageResult = Page.Create(page);
        if (pageResult.IsFailure) return pageResult.Error;

        var assetsResult = await assetRepository.GetAllPagesAsync(pageResult.Value);
        if (assetsResult.IsFailure) return assetsResult.Error;

        return new GetAssetsMetadataResponse
        {
            Assets = assetsResult.Value.Items
                .Select(i => i.ToMetadataResponse())
                .ToList(),
            Page = assetsResult.Value.Page,
            PageSize = assetsResult.Value.PageSize,
            TotalCount = assetsResult.Value.TotalCount,
            HasNextPage = assetsResult.Value.HasNextPage,
        };
    }
}