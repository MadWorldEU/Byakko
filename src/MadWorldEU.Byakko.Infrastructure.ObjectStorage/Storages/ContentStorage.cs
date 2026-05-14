using Microsoft.Extensions.Logging;

namespace MadWorldEU.Byakko.Storages;

public sealed class ContentStorage(IAmazonS3 s3Client, ILogger<ContentStorage> logger) : IContentStorage
{
    public async Task<Result> UploadAsync(AssetPath filePath, Stream stream)
    {
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = filePath.Path,
                Key = filePath.Key,
                InputStream = stream,
                AutoCloseStream = false
            };

            await s3Client.PutObjectAsync(request);
            
            logger.LogInformation("Object uploaded to bucket '{BucketName}' with key '{Key}'.", filePath.Path, filePath.Key);
            return Result.Success();
        }
        catch (AmazonS3Exception exception)
        {
            logger.LogError(exception, "Failed to upload object to bucket '{BucketName}' with key '{Key}'.", filePath.Path, filePath.Key);
            return Result.Failure(ContentStorageErrors.UploadFailed);
        }
    }
}