namespace MadWorldEU.Byakko.Storages;

public interface IContentStorage
{
    Task<Result> UploadAsync(AssetPath filePath, Stream stream);
}