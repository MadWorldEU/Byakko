using MadWorldEU.Byakko.Audits;
using MadWorldEU.Byakko.DomainDrivenDevelopment;
using MadWorldEU.Byakko.Systems;
using Microsoft.Extensions.Options;

namespace MadWorldEU.Byakko.Storages;

/// <summary>Creates a new asset metadata record.</summary>
public sealed class CreateAssetMetadataUseCase(
    IClock clock, 
    IGuidGenerator guidGenerator, 
    IAssetRepository repository, 
    IDomainEventsDispatcher domainEventsDispatcher, 
    IOptions<AssetSettings> settings)
{
    public async Task<Result<CreateAssetResponse>> ExecuteAsync(CreateAssetRequest request, string userId, System.Net.IPAddress? ipAddress)
    {
        var nameResult = Name.Create(request.Name);
        if (nameResult.IsFailure) return nameResult.Error;

        var contentTypeResult = ContentType.Create(request.ContentType);
        if (contentTypeResult.IsFailure) return contentTypeResult.Error;

        var userIdResult = UserId.Create(userId);
        if (userIdResult.IsFailure) return userIdResult.Error;
        
        var ipAddressResult = IpAddress.Create(ipAddress);
        if (ipAddressResult.IsFailure) return ipAddressResult.Error;

        var sizeResult = Size.Create(request.Size);
        if (sizeResult.IsFailure) return sizeResult.Error;
        if (sizeResult.Value > settings.Value.MaxUploadSizeInBytes) return AssetErrors.FileTooLarge;

        var countActiveAssetsResult = await repository.GetCountOfActiveAssetsAsync(userIdResult.Value);
        if (countActiveAssetsResult.IsFailure) return countActiveAssetsResult.Error;
        if (countActiveAssetsResult.Value >= settings.Value.MaxFilesEachUser) return AssetErrors.MaxFilesReached;

        var validityPeriodResult = ValidityPeriod.Create(settings.Value.ValidityPeriodInDays);
        if (validityPeriodResult.IsFailure) return validityPeriodResult.Error;

        var assetResult = Asset.Create(clock, guidGenerator, nameResult.Value, contentTypeResult.Value, userIdResult.Value, validityPeriodResult.Value);
        if (assetResult.IsFailure) return assetResult.Error;

        var saveResult = await repository.AddAsync(assetResult.Value);
        if (saveResult.IsFailure) return saveResult.Error;

        var assetMetaDataCreatedEvent = new AssetMetaDataCreatedEvent(assetResult.Value.Id, ipAddressResult.Value, assetResult.Value.CreatedBy, assetResult.Value.CreatedAt);
        await domainEventsDispatcher.DispatchAsync([assetMetaDataCreatedEvent]);

        return new CreateAssetResponse { Id = assetResult.Value.Id.Value };
    }
}