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
    public async Task<GetUserUploadLimitsResponse?> GetUserUploadLimitsAsync()
    {
        return await _httpClientAuthorized.GetFromJsonAsync<GetUserUploadLimitsResponse>("/assets/limits");
    }

    /// <inheritdoc />
    public async Task<CreateAssetResponse?> CreateAssetAsync(CreateAssetRequest request)
    {
        var response = await _httpClientAuthorized.PostAsJsonAsync("/assets", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CreateAssetResponse>();
    }

    /// <inheritdoc />
    public async Task<GetAssetsMetadataResponse?> GetAssetsMetadataAsync(int page, Guid? assetId = null, Guid? userId = null)
    {
        var url = $"/assets?page={page}";
        if (assetId.HasValue) url += $"&assetId={assetId}";
        if (userId.HasValue) url += $"&userId={userId}";
        return await _httpClientAuthorized.GetFromJsonAsync<GetAssetsMetadataResponse>(url);
    }

    /// <inheritdoc />
    public async Task<GetAssetsMetadataResponse?> GetMyAssetsMetadataAsync(int page)
    {
        return await _httpClientAuthorized.GetFromJsonAsync<GetAssetsMetadataResponse>($"/assets/me?page={page}");
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
    public async Task DeleteAssetContentAsync(Guid id)
    {
        var response = await _httpClientAuthorized.DeleteAsync($"/assets/{id}/content");
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc />
    public async Task DeleteMyAssetContentAsync(Guid id)
    {
        var response = await _httpClientAuthorized.DeleteAsync($"/assets/me/{id}/content");
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc />
    public string GetContentUrl(Guid id)
    {
        var baseAddress = _httpClientAnonymous.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;
        return $"{baseAddress}/assets/{id}/content";
    }
}