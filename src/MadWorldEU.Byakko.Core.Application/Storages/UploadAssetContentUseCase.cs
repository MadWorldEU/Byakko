using MadWorldEU.Byakko.Audits;
using MadWorldEU.Byakko.DomainDrivenDevelopment;
using MadWorldEU.Byakko.Encryptions;
using Microsoft.Extensions.Options;

namespace MadWorldEU.Byakko.Storages;

public sealed class UploadAssetContentUseCase(
    IClock clock,
    IEncryptionService encryptionService,
    IAssetRepository assetRepository,
    IContentStorage contentStorage,
    IDomainEventsDispatcher domainEventsDispatcher,
    IAssetMetrics metrics,
    IOptions<AssetSettings> settings)
{
    public async Task<Result<UploadAssetContentResponse>> ExecuteAsync(
        string assetId, 
        FileRequest fileRequest,
        string userId, 
        System.Net.IPAddress? ipAddress, 
        string? password)
    {
        var id = Id.Create(assetId);
        if (id.IsFailure) return id.Error;

        var sizeResult = Size.Create(fileRequest.SizeInBytes);
        if (sizeResult.IsFailure) return sizeResult.Error;
        if (sizeResult.Value > settings.Value.MaxUploadSizeInBytes) return AssetErrors.FileTooLarge;
        
        var userIdResult = UserId.Create(userId);
        if (userIdResult.IsFailure) return userIdResult.Error;
        
        var ipAddressResult = IpAddress.Create(ipAddress);
        if (ipAddressResult.IsFailure) return ipAddressResult.Error;

        var passwordResult = Password.Create(password);
        
        var asset = await assetRepository.FindAsync(id.Value);
        if (asset.IsFailure) return asset.Error;

        if (asset.Value.CreatedBy != userIdResult.Value)
        {
            return AssetErrors.Forbidden;
        }

        if (asset.Value.Name.Value != fileRequest.FileName)
        {
            return AssetErrors.FileNameMismatch;
        }

        if (!asset.Value.ContentType.Value.Equals(fileRequest.ContentType, StringComparison.OrdinalIgnoreCase))
        {
            return AssetErrors.ContentTypeMismatch;
        }

        var encryptedContent = encryptionService.Encrypt(fileRequest.Content, passwordResult.Value);
        
        var uploadResult = await contentStorage.UploadAsync(asset.Value.GetPath(), encryptedContent);
        if (uploadResult.IsFailure) return uploadResult.Error;

        var updateSizeResult = asset.Value.UpdateSize(clock, sizeResult.Value);
        if (updateSizeResult.IsFailure) return updateSizeResult.Error;

        var updateResult = await assetRepository.UpdateAsync(asset.Value);
        if (updateResult.IsFailure) return updateResult.Error;
        
        var assetMetaDataCreatedEvent = new AssetContentUploadedEvent(asset.Value.Id, ipAddressResult.Value, asset.Value.CreatedBy, asset.Value.CreatedAt);
        await domainEventsDispatcher.DispatchAsync([assetMetaDataCreatedEvent]);

        metrics.RecordUpload();
        return new UploadAssetContentResponse { Id = asset.Value.Id.Value };
    }
}