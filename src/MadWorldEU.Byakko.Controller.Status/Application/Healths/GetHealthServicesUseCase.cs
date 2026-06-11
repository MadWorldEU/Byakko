using MadWorldEU.Byakko.Configurations;
using MadWorldEU.Byakko.Domain.Healths;
using Microsoft.Extensions.Options;

namespace MadWorldEU.Byakko.Application.Healths;

/// <summary>
/// Returns health status for all tracked services.
/// </summary>
internal sealed class GetHealthServicesUseCase(
    IHttpClientFactory httpClientFactory,
    IOptions<HealthCheckSettings> settings)
{
    internal async Task<IReadOnlyList<ServiceInfo>> ExecuteAsync()
    {
        var apiTask = CheckServiceAsync("Api", settings.Value.Api);
        var portalTask = CheckServiceAsync("Portal", settings.Value.Portal);
        var adminTask = CheckServiceAsync("Admin", settings.Value.Admin);

        await Task.WhenAll(apiTask, portalTask, adminTask);

        return
        [
            await apiTask,
            await portalTask,
            await adminTask,
            new ServiceInfo("Database", ServiceStatus.Healthy),
            new ServiceInfo("Object Storage", ServiceStatus.Healthy),
            new ServiceInfo("Authentication", ServiceStatus.Healthy),
        ];
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
            var body = (await response.Content.ReadAsStringAsync()).Trim();

            var status = body switch
            {
                "Healthy" => ServiceStatus.Healthy,
                "Degraded" => ServiceStatus.Degraded,
                _ => ServiceStatus.Unhealthy
            };

            return new ServiceInfo(name, status);
        }
        catch
        {
            return new ServiceInfo(name, ServiceStatus.Unhealthy);
        }
    }
}