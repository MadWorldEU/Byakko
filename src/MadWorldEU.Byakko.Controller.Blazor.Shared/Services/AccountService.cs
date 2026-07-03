using MadWorldEU.Byakko.Accounts;
using MadWorldEU.Byakko.Common;

namespace MadWorldEU.Byakko.Services;

/// <summary>HTTP client wrapper for the Accounts API endpoints.</summary>
public sealed class AccountService(IHttpClientFactory httpClientFactory) : IAccountService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(HttpClients.ApiAuthorized);

    /// <inheritdoc />
    public async Task<ResultResponse<CreateMyAccountResponse>> CreateMyAccountAsync()
    {
        return await _httpClient.PostResultResponseFromJsonAsync<object, CreateMyAccountResponse>("/accounts/me", new { });
    }

    /// <inheritdoc />
    public async Task<ResultResponse<GetMyAccountResponse>> GetMyAccountAsync()
    {
        return await _httpClient.GetResultResponseFromJsonAsync<GetMyAccountResponse>("/accounts/me");
    }

    /// <inheritdoc />
    public async Task<ResultResponse<RequestDeletionMyAccountResponse>> RequestDeletionMyAccountAsync()
    {
        return await _httpClient.PostResultResponseFromJsonAsync<object, RequestDeletionMyAccountResponse>("/accounts/me/deletion-request", new { });
    }
}