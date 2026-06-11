using MadWorldEU.Byakko.Configurations;
using MadWorldEU.Byakko.Domain.Healths;
using MadWorldEU.Byakko.Infrastructure.Postgresql;
using Microsoft.Extensions.Options;

namespace MadWorldEU.Byakko.Application.Healths;

/// <summary>
/// Returns health status for all tracked services.
/// </summary>
internal sealed class GetHealthServicesUseCase(
    ByakkoContext context,
    IHttpClientFactory httpClientFactory,
    IOptions<HealthCheckSettings> settings,
    ILogger<GetHealthServicesUseCase> logger)
{
    internal async Task<IReadOnlyList<ServiceInfo>> ExecuteAsync()
    {
        var apiTask = CheckServiceAsync("API", settings.Value.Api);
        var portalTask = CheckServiceAsync("Portal", settings.Value.Portal);
        var adminTask = CheckServiceAsync("Admin", settings.Value.Admin);
        var databaseTask = CheckDatabaseAsync("Database");
        var authTask = CheckServiceAsync("Authentication", settings.Value.Authentication);

        await Task.WhenAll(apiTask, portalTask, adminTask, authTask);

        return
        [
            await apiTask,
            await portalTask,
            await adminTask,
            await databaseTask,
            new ServiceInfo("Object Storage", ServiceStatus.Healthy),
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
    
    private async Task<ServiceInfo> CheckServiceAsync(string name, string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return new ServiceInfo(name, ServiceStatus.Unknown);
        }

        try
        {
            var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(2);
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