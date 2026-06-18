using System.Net.Http.Json;
using MadWorldEU.Byakko.Audits;
using MadWorldEU.Byakko.Common;

namespace MadWorldEU.Byakko.Responses;

public static class HttpClientExtensions
{
    public static async Task<ResultResponse<EmptyResponse>> DeleteResultResponseFromJsonAsync(this HttpClient httpClient, string requestUri)
    {
        var response = await httpClient.DeleteAsync(requestUri);

        if (response.IsSuccessStatusCode)
        {
            return new EmptyResponse();
        }

        return (await response.Content.ReadFromJsonAsync<FailureResponse>())!;
    }
    
    public static async Task<ResultResponse<TResponse>> GetResultResponseFromJsonAsync<TResponse>(this HttpClient httpClient, string requestUri)
    {
        var response = await httpClient.GetAsync(requestUri);

        if (response.IsSuccessStatusCode)
        {
            return (await response.Content.ReadFromJsonAsync<TResponse>())!;
        }

        return (await response.Content.ReadFromJsonAsync<FailureResponse>())!;
    }

    public static async Task<ResultResponse<TResponse>> PostResultResponseFromJsonAsync<TRequest, TResponse>(this HttpClient httpClient, string requestUri, TRequest request)
    {
        var response = await httpClient.PostAsJsonAsync(requestUri, request);
        if (response.IsSuccessStatusCode)
        {
            return (await response.Content.ReadFromJsonAsync<TResponse>())!;
        }

        return (await response.Content.ReadFromJsonAsync<FailureResponse>())!;
    }

    public static async Task<ResultResponse<TResponse>> PutResultResponseFromJsonAsync<TResponse>(this HttpClient httpClient, string requestUri, HttpContent content)
    {
        var response = await httpClient.PutAsync(requestUri, content);
        if (response.IsSuccessStatusCode)
        {
            return (await response.Content.ReadFromJsonAsync<TResponse>())!;
        }

        return (await response.Content.ReadFromJsonAsync<FailureResponse>())!;
    }
}