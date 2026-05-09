namespace MadWorldEU.Byakko.Storages;

/// <summary>Creates a new asset metadata record.</summary>
public sealed class CreateAssetMetadataUseCase(IAssetRepository repository)
{
    public async Task<Result<CreateAssetResponse>> ExecuteAsync(CreateAssetRequest request)
    {
        var nameResult = Name.Create(request.Name);
        if (nameResult.IsFailure) return nameResult.Error;

        var assetResult = Asset.Create(nameResult.Value);
        if (assetResult.IsFailure) return assetResult.Error;

        var saveResult = await repository.AddAsync(assetResult.Value);
        if (saveResult.IsFailure) return saveResult.Error;

        return new CreateAssetResponse { Id = assetResult.Value.Id };
    }
}