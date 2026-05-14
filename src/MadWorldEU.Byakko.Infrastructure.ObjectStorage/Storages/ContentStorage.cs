using Microsoft.Extensions.Logging;

namespace MadWorldEU.Byakko.Storages;

/// <summary>S3-compatible object storage implementation for uploading and downloading asset content.</summary>
public sealed class ContentStorage(IAmazonS3 s3Client, ILogger<ContentStorage> logger) : IContentStorage
{
    /// <summary>Uploads a stream to object storage at the given path.</summary>
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

    /// <summary>Downloads the content stream for the asset at the given path.</summary>
    public async Task<Result<Stream>> DownloadAsync(AssetPath filePath)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = filePath.Path,
                Key = filePath.Key
            };

            var response = await s3Client.GetObjectAsync(request);

            logger.LogInformation("Object downloaded from bucket '{BucketName}' with key '{Key}'.", filePath.Path, filePath.Key);
            return response.ResponseStream;
        }
        catch (AmazonS3Exception exception)
        {
            logger.LogError(exception, "Failed to download object from bucket '{BucketName}' with key '{Key}'.", filePath.Path, filePath.Key);
            return ContentStorageErrors.DownloadFailed;
        }
    }
}