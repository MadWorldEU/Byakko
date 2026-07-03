using MadWorldEU.Byakko.Accounts;

namespace MadWorldEU.Byakko.Services;

/// <summary>HTTP client wrapper for the Accounts API endpoints.</summary>
public sealed class AccountService(IHttpClientFactory httpClientFactory) : IAccountService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(HttpClients.ApiAuthorized);

    /// <inheritdoc />
    public async Task<ResultResponse<GetDeleteRequestedAccountsResponse>> GetDeleteRequestedAccountsAsync(int page)
    {
        return await _httpClient.GetResultResponseFromJsonAsync<GetDeleteRequestedAccountsResponse>($"/accounts?page={page}");
    }

    /// <inheritdoc />
    public async Task<ResultResponse<CreateMyAccountResponse>> CreateMyAccountAsync()
    {
        return await _httpClient.PostResultResponseFromJsonAsync<EmptyRequest, CreateMyAccountResponse>("/accounts/me", EmptyRequest.Create());
    }

    /// <inheritdoc />
    public async Task<ResultResponse<GetMyAccountResponse>> GetMyAccountAsync()
    {
        return await _httpClient.GetResultResponseFromJsonAsync<GetMyAccountResponse>("/accounts/me");
    }

    /// <inheritdoc />
    public async Task<ResultResponse<RequestDeletionMyAccountResponse>> RequestDeletionMyAccountAsync()
    {
        return await _httpClient.PostResultResponseFromJsonAsync<EmptyRequest, RequestDeletionMyAccountResponse>("/accounts/me/deletion-request", EmptyRequest.Create());
    }
}