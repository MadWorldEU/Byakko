using MadWorldEU.Byakko.Storages;

namespace MadWorldEU.Byakko.Services;

/// <summary>Wraps the Assets API endpoints for use in Blazor WebAssembly applications.</summary>
public interface IAssetService
{
    /// <summary>Creates a new asset metadata record and returns the assigned ID.</summary>
    Task<CreateAssetResponse?> CreateAssetAsync(CreateAssetRequest request);

    /// <summary>Returns a paged list of asset metadata for administrators. Optionally filters by asset ID or user ID.</summary>
    Task<GetAssetsMetadataResponse?> GetAssetsMetadataAsync(int page, Guid? assetId = null, Guid? userId = null);

    /// <summary>Returns the metadata of an asset by ID.</summary>
    Task<GetAssetMetadataResponse?> GetAssetMetadataAsync(Guid id);

    /// <summary>Uploads binary content for an existing asset.</summary>
    Task<UploadAssetContentResponse?> UploadAssetContentAsync(Guid id, Stream content, string fileName, string contentType);

    /// <summary>Deletes the content of an asset by ID. Throws <see cref="HttpRequestException"/> on failure.</summary>
    Task DeleteAssetContentAsync(Guid id);

    /// <summary>Returns the direct URL to download the binary content of an asset. The browser streams it natively — no in-memory buffering.</summary>
    string GetContentUrl(Guid id);
}