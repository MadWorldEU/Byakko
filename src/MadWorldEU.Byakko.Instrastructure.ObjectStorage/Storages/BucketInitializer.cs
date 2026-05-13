using Microsoft.Extensions.Logging;

namespace MadWorldEU.Byakko.Storages;

/// <summary>Ensures the configured S3 bucket exists before the application starts accepting requests.</summary>
internal sealed class BucketInitializer(IAmazonS3 s3Client, ILogger<BucketInitializer> logger) : IHostedService
{
    private readonly string[] _bucketNames = [ Asset.DefaultPath ];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var bucketName in _bucketNames)
        {
            await CheckAndCreateBucketAsync(cancellationToken, bucketName);
        }
    }

    private async Task CheckAndCreateBucketAsync(CancellationToken cancellationToken, string bucketName)
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