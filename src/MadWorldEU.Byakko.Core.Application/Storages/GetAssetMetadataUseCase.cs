namespace MadWorldEU.Byakko.Storages;

/// <summary>Retrieves the metadata of an existing asset by its identifier.</summary>
public sealed class GetAssetMetadataUseCase(IAssetRepository assetRepository)
{
    public async Task<Result<GetAssetMetadataResponse>> ExecuteAsync(string assetId)
    {
        var id = Id.Create(assetId);
        if (id.IsFailure) return id.Error;

        var asset = await assetRepository.FindAsync(id.Value);
        if (asset.IsFailure) return asset.Error;

        return new GetAssetMetadataResponse
        {
            Id = asset.Value.Id.Value,
            Name = asset.Value.Name.Value,
            ContentType = asset.Value.ContentType.Value,
            CreatedAt = asset.Value.CreatedAt.ToDateTimeOffset(),
            UpdatedAt = asset.Value.UpdatedAt.ToDateTimeOffset(),
            ExpiresAt = asset.Value.ExpiresAt.ToDateTimeOffset()
        };
    }
}