using MadWorldEU.Byakko.Systems;
using Microsoft.Extensions.Options;

namespace MadWorldEU.Byakko.Storages;

/// <summary>Creates a new asset metadata record.</summary>
public sealed class CreateAssetMetadataUseCase(IClock clock, IGuidGenerator guidGenerator, IAssetRepository repository, IOptions<AssetSettings> settings)
{
    public async Task<Result<CreateAssetResponse>> ExecuteAsync(CreateAssetRequest request, string userId)
    {
        var nameResult = Name.Create(request.Name);
        if (nameResult.IsFailure) return nameResult.Error;

        var contentTypeResult = ContentType.Create(request.ContentType);
        if (contentTypeResult.IsFailure) return contentTypeResult.Error;

        var userIdResult = UserId.Create(userId);
        if (userIdResult.IsFailure) return userIdResult.Error;

        var validityPeriodResult = ValidityPeriod.Create(settings.Value.ValidityPeriodInDays);
        if (validityPeriodResult.IsFailure) return validityPeriodResult.Error;

        var assetResult = Asset.Create(clock, guidGenerator, nameResult.Value, contentTypeResult.Value, userIdResult.Value, validityPeriodResult.Value);
        if (assetResult.IsFailure) return assetResult.Error;

        var saveResult = await repository.AddAsync(assetResult.Value);
        if (saveResult.IsFailure) return saveResult.Error;

        return new CreateAssetResponse { Id = assetResult.Value.Id.Value };
    }
}