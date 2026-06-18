using MadWorldEU.Byakko.Encryptions;

namespace MadWorldEU.Byakko.Storages;

/// <summary>Downloads the binary content of an asset from object storage.</summary>
public sealed class DownloadAssetContentUseCase(IClock clock, IEncryptionService encryptionService, IAssetRepository assetRepository, IContentStorage contentStorage)
{
    public async Task<Result<DownloadAssetContentResponse>> QueryAsync(string assetId, string? password)
    {
        var id = Id.Create(assetId);
        if (id.IsFailure) return id.Error;

        var asset = await assetRepository.FindAsync(id.Value);
        if (asset.IsFailure) return asset.Error;

        var passwordResult = Password.Create(password);
        if (passwordResult.IsFailure) return passwordResult.Error;
        
        if (asset.Value.IsExpired(clock))
        {
            return AssetErrors.Expired;
        }

        var encryptedContent = await contentStorage.DownloadAsync(asset.Value.GetPath());
        if (encryptedContent.IsFailure) return encryptedContent.Error;

        var content = encryptionService.Decrypt(encryptedContent.Value, passwordResult.Value);
        if (content.IsFailure) return content.Error;
        
        return new DownloadAssetContentResponse
        {
            Content = content.Value,
            ContentType = asset.Value.ContentType.Value,
            FileName = asset.Value.Name.Value
        };
    }
}