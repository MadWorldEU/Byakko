namespace MadWorldEU.Byakko.Storages;

public sealed class UploadAssetContentUseCase(IAssetRepository assetRepository, IContentStorage contentStorage)
{
    public IAssetRepository AssetRepository { get; } = assetRepository;
    public IContentStorage ContentStorage { get; } = contentStorage;

    public void ExecuteAsync()
    {
        
    }
}