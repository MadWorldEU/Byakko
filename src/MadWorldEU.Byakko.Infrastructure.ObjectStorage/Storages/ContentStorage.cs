using Microsoft.Extensions.Logging;

namespace MadWorldEU.Byakko.Storages;

/// <summary>S3-compatible object storage implementation for uploading and downloading asset content.</summary>
internal sealed class ContentStorage(IAmazonS3 s3Client, IOptions<StorageOptions> options, ILogger<ContentStorage> logger) : IContentStorage
{
    private readonly string _bucketName = options.Value.BucketName;

    /// <summary>Uploads a stream to object storage at the given path.</summary>
    public async Task<Result> UploadAsync(AssetPath filePath, Stream stream)
    {
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = filePath.FullPath,
                InputStream = stream,
                AutoCloseStream = false
            };

            await s3Client.PutObjectAsync(request);

            logger.LogInformation("Object uploaded to bucket '{BucketName}' with key '{Key}'.", _bucketName, filePath.Key);
            return Result.Success();
        }
        catch (AmazonS3Exception exception)
        {
            logger.LogError(exception, "Failed to upload object to bucket '{BucketName}' with key '{Key}'.", _bucketName, filePath.Key);
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
                BucketName = _bucketName,
                Key = filePath.FullPath
            };

            var response = await s3Client.GetObjectAsync(request);

            logger.LogInformation("Object downloaded from bucket '{BucketName}' with key '{Key}'.", _bucketName, filePath.Key);
            return response.ResponseStream;
        }
        catch (AmazonS3Exception exception)
        {
            logger.LogError(exception, "Failed to download object from bucket '{BucketName}' with key '{Key}'.", _bucketName, filePath.Key);
            return ContentStorageErrors.DownloadFailed;
        }
    }

    /// <summary>Deletes the object at the given path from storage.</summary>
    public async Task<Result> DeleteAsync(AssetPath filePath)
    {
        try
        {
            var metadata = await s3Client.GetObjectMetadataAsync(_bucketName, filePath.FullPath);

            await s3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = filePath.FullPath,
                VersionId = metadata.VersionId
            });

            logger.LogInformation("Object deleted from bucket '{BucketName}' with key '{Key}'.", _bucketName,
                filePath.Key);
            return Result.Success();
        }
        catch (AmazonS3Exception exception) when (exception.ErrorCode == "NotFound")
        {
            logger.LogWarning("Object not found in bucket '{BucketName}' with key '{Key}'. It may have already been deleted.", _bucketName,
                filePath.Key);
            return Result.Success();
        }
        catch (AmazonS3Exception exception)
        {
            logger.LogError(exception, "Failed to delete object from bucket '{BucketName}' with key '{Key}'.", _bucketName, filePath.Key);
            return Result.Failure(ContentStorageErrors.DeleteFailed);
        }
    }
}