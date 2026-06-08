using System.Net.Http.Json;
using MadWorldEU.Byakko.Storages;

namespace MadWorldEU.Byakko.Services;

/// <summary>HTTP client wrapper for the general Storage API endpoints.</summary>
public sealed class StorageService : IStorageService
{
    private readonly HttpClient _httpClient;

    /// <summary>Initialises the service with the authorised HTTP client.</summary>
    public StorageService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(HttpClients.ApiAuthorized);
    }

    /// <inheritdoc />
    public async Task<GetStorageStatisticsResponse?> GetStorageStatisticsAsync()
    {
        return await _httpClient.GetFromJsonAsync<GetStorageStatisticsResponse>("/storage/statistics");
    }
}