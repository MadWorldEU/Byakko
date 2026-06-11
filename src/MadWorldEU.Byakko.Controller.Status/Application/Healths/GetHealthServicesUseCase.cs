using Amazon.S3;
using MadWorldEU.Byakko.Configurations;
using MadWorldEU.Byakko.Healths;
using Microsoft.Extensions.Options;

namespace MadWorldEU.Byakko.Application.Healths;

/// <summary>
/// Returns health status for all tracked services.
/// </summary>
internal sealed class GetHealthServicesUseCase(
    ByakkoContext context,
    IAmazonS3 s3Client,
    IHttpClientFactory httpClientFactory,
    IOptions<HealthCheckSettings> settings,
    ILogger<GetHealthServicesUseCase> logger)
{
    private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(2);
    
    internal async Task<IReadOnlyList<ServiceInfo>> ExecuteAsync()
    {
        var apiTask = CheckServiceAsync("API", settings.Value.Api);
        var portalTask = CheckServiceAsync("Portal", settings.Value.Portal);
        var adminTask = CheckServiceAsync("Admin", settings.Value.Admin);
        var databaseTask = CheckDatabaseAsync("Database");
        var objectStorageTask = CheckObjectStorageAsync("Object Storage");
        var authTask = CheckServiceAsync("Authentication", settings.Value.Authentication);

        await Task.WhenAll(apiTask, portalTask, adminTask, databaseTask, objectStorageTask, authTask);

        return
        [
            await apiTask,
            await portalTask,
            await adminTask,
            await databaseTask,
            await objectStorageTask,
            await authTask,
        ];
    }

    private async Task<ServiceInfo> CheckDatabaseAsync(string name)
    {
        try
        {
            var canConnect = await context.Database.CanConnectAsync();

            if (canConnect)
            {
                return new ServiceInfo(name, ServiceStatus.Healthy);
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Database connection check failed");
        }
        
        return new ServiceInfo(name, ServiceStatus.Unhealthy);
    }

    private async Task<ServiceInfo> CheckObjectStorageAsync(string name)
    {
        using var cts = new CancellationTokenSource(_defaultTimeout);

        try
        {
            await s3Client.ListBucketsAsync(cts.Token);
            return new ServiceInfo(name, ServiceStatus.Healthy);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Object storage connection check failed");
            return new ServiceInfo(name, ServiceStatus.Unhealthy);
        }
    }

    private async Task<ServiceInfo> CheckServiceAsync(string name, string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return new ServiceInfo(name, ServiceStatus.Unknown);
        }

        try
        {
            var client = httpClientFactory.CreateClient();
            client.Timeout = _defaultTimeout;
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return new ServiceInfo(name, ServiceStatus.Unhealthy);
            }

            var body = (await response.Content.ReadAsStringAsync()).Trim();
            var status = body == "Degraded" ? ServiceStatus.Degraded : ServiceStatus.Healthy;

            return new ServiceInfo(name, status);
        }
        catch
        {
            return new ServiceInfo(name, ServiceStatus.Unhealthy);
        }
    }
}