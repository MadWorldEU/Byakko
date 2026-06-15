namespace MadWorldEU.Byakko.Storages;

/// <summary>Returns a paged list of asset metadata records.</summary>
public sealed class GetAssetsMetaDataUseCase(IAssetRepository assetRepository)
{
    public async Task<Result<GetAssetsMetadataResponse>> QueryAsync(int page, Guid? assetId = null, Guid? userId = null)
    {
        var pageResult = Page.Create(page);
        if (pageResult.IsFailure) return pageResult.Error;
        
        var idResult = assetId == null ? Id.Empty : Id.Create(assetId.Value);
        if (idResult.IsFailure) return idResult.Error;
        
        var userIdResult = userId == null ? UserId.Empty : UserId.Create(userId.Value);
        if (userIdResult.IsFailure) return userIdResult.Error;
        
        var assetsResult = await assetRepository.GetAllPagesAsync(idResult.Value, userIdResult.Value, pageResult.Value);
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