using System.Net.Http.Json;
using MadWorldEU.Byakko.Audits;
using MadWorldEU.Byakko.Common;

namespace MadWorldEU.Byakko.Responses;

public static class HttpClientExtensions
{
    public static async Task<ResultResponse<TResponse>> GetResultResponseFromJsonAsync<TResponse>(this HttpClient httpClient, string requestUri)
    {
        var response = await httpClient.GetAsync(requestUri);

        if (response.IsSuccessStatusCode)
        {
            return (await response.Content.ReadFromJsonAsync<TResponse>())!;
        }

        return (await response.Content.ReadFromJsonAsync<FailureResponse>())!;
    }
}