using Microsoft.Extensions.Logging;

namespace MadWorldEU.Byakko.Storages;

/// <summary>Ensures the configured S3 bucket exists before the application starts accepting requests.</summary>
internal sealed class BucketInitializer(IAmazonS3 s3Client, IOptions<StorageOptions> options, ILogger<BucketInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.AutoCreateBucket)
        {
            return;
        }

        await CheckAndCreateBucketAsync(options.Value.BucketName, cancellationToken);
    }

    private async Task CheckAndCreateBucketAsync(string bucketName, CancellationToken cancellationToken)
    {
        try
        {
            var exists = await AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName);
            if (!exists)
            {
                await s3Client.PutBucketAsync(new PutBucketRequest { BucketName = bucketName }, cancellationToken);
                logger.LogInformation("Bucket '{BucketName}' created successfully.", bucketName);
            }
        }
        catch (AmazonS3Exception exception)
        {
            logger.LogError(exception, "Failed to ensure bucket '{BucketName}' exists.", bucketName);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}