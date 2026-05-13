namespace MadWorldEU.Byakko.Storages;

public class ContentStorage(IAmazonS3 s3Client) : IContentStorage
{
    public void Upload()
    {
        _ = s3Client.Config;
    }
}