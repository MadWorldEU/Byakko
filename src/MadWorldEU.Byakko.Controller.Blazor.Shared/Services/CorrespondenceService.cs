using System.Net;
using System.Net.Http.Json;
using MadWorldEU.Byakko.Common;
using MadWorldEU.Byakko.Configurations;
using MadWorldEU.Byakko.Correspondences;

namespace MadWorldEU.Byakko.Services;

/// <summary>HTTP client wrapper for the Correspondences API endpoints.</summary>
public sealed class CorrespondenceService(IHttpClientFactory httpClientFactory) : ICorrespondenceService
{
    private readonly HttpClient _httpClientAnonymous = httpClientFactory.CreateClient(HttpClients.ApiAnonymous);
    private readonly HttpClient _httpClientAuthorized = httpClientFactory.CreateClient(HttpClients.ApiAuthorized);

    /// <inheritdoc />
    public async Task<ResultResponse<EmptyResponse>> SendFeedbackAsync(SendFeedbackRequest request, bool isAuthenticated)
    {
        var httpClient = GetHttpClient(isAuthenticated);
        var response = await httpClient.PostAsJsonAsync("/correspondences/feedback", request);

        if (response.IsSuccessStatusCode)
        {
            return new EmptyResponse();
        }

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            return new FailureResponse
            {
                StatusCode = (int)response.StatusCode,
                Code = BlazorErrors.CorrespondenceTooManyRequests.Code,
                Description = BlazorErrors.CorrespondenceTooManyRequests.Description
            };
        }

        return (await response.Content.ReadFromJsonAsync<FailureResponse>())!;
    }
    
    private HttpClient GetHttpClient(bool isAuthenticated)
    {
        return isAuthenticated ? _httpClientAuthorized : _httpClientAnonymous;
    }
}