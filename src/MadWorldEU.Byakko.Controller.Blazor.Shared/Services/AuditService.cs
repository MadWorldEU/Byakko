using System.Net.Http.Json;
using MadWorldEU.Byakko.Audits;

namespace MadWorldEU.Byakko.Services;

/// <summary>HTTP client wrapper for the Audits API endpoints.</summary>
public sealed class AuditService(IHttpClientFactory httpClientFactory) : IAuditService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(HttpClients.ApiAuthorized);

    /// <inheritdoc />
    public async Task<GetAuditLogsResponse?> GetAuditLogsAsync(Guid entityId)
    {
        return await _httpClient.GetFromJsonAsync<GetAuditLogsResponse>($"/audits/{entityId}");
    }
}