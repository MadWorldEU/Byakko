using System.Net.Http.Headers;
using System.Text.Json;
using MadWorldEU.Byakko.Configurations;
using Microsoft.Extensions.Options;

namespace MadWorldEU.Byakko.AuthenticationServers;

/// <summary>Fetches a client-credentials token from Keycloak and attaches it to admin API requests.</summary>
internal sealed class KeyCloakTokenHandler(
    IOptions<KeyCloakSettings> settings,
    IHttpClientFactory httpClientFactory) : DelegatingHandler
{
    private readonly KeyCloakSettings _settings = settings.Value;

    private string? _cachedToken;
    private DateTimeOffset _tokenExpiry = DateTimeOffset.MinValue;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await GetTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        if (_cachedToken is not null && DateTimeOffset.UtcNow < _tokenExpiry)
        {
            return _cachedToken;
        }

        var client = httpClientFactory.CreateClient();
        var tokenEndpoint = $"{_settings.AuthServerUrl.TrimEnd('/')}/realms/master/protocol/openid-connect/token";

        var response = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _settings.Resource,
                ["client_secret"] = _settings.AdminClientSecret
            }), cancellationToken);

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var json = JsonSerializer.Deserialize<JsonElement>(body);
        _cachedToken = json.GetProperty("access_token").GetString()!;
        var expiresIn = json.GetProperty("expires_in").GetInt32();
        _tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(expiresIn - 30);

        return _cachedToken;
    }
}