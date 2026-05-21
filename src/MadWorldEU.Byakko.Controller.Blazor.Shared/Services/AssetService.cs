using System.Net.Http.Json;
using MadWorldEU.Byakko.Storages;

namespace MadWorldEU.Byakko.Services;

/// <summary>HTTP client wrapper for the Assets API endpoints.</summary>
public sealed class AssetService : IAssetService
{
    private readonly HttpClient _httpClientAnonymous;
    private readonly HttpClient _httpClientAuthorized;

    /// <summary>Initialises the service with the authorised HTTP client.</summary>
    public AssetService(IHttpClientFactory httpClientFactory)
    {
        _httpClientAnonymous  = httpClientFactory.CreateClient(HttpClients.ApiAnonymous);
        _httpClientAuthorized = httpClientFactory.CreateClient(HttpClients.ApiAuthorized);
    }

    /// <inheritdoc />
    public async Task<CreateAssetResponse?> CreateAssetAsync(CreateAssetRequest request)
    {
        var response = await _httpClientAuthorized.PostAsJsonAsync("/assets", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CreateAssetResponse>();
    }

    /// <inheritdoc />
    public async Task<GetAssetMetadataResponse?> GetAssetMetadataAsync(Guid id)
    {
        return await _httpClientAnonymous.GetFromJsonAsync<GetAssetMetadataResponse>($"/assets/{id}");
    }

    /// <inheritdoc />
    public async Task<UploadAssetContentResponse?> UploadAssetContentAsync(Guid id, Stream content, string fileName, string contentType)
    {
        using var form = new MultipartFormDataContent();
        using var streamContent = new StreamContent(content);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        form.Add(streamContent, "file", fileName);

        var response = await _httpClientAuthorized.PutAsync($"/assets/{id}/content", form);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UploadAssetContentResponse>();
    }

    /// <inheritdoc />
    public async Task<(byte[] Content, string ContentType, string FileName)?> DownloadAssetContentAsync(Guid id)
    {
        var response = await _httpClientAnonymous.GetAsync($"/assets/{id}/content");
        response.EnsureSuccessStatusCode();

        var fileContent = await response.Content.ReadAsByteArrayAsync();
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName
            ?? "download";

        return (fileContent, contentType, fileName);
    }
}