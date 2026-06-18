using MadWorldEU.Byakko.Common;
using MadWorldEU.Byakko.Storages;

namespace MadWorldEU.Byakko.Services;

/// <summary>Wraps the Assets API endpoints for use in Blazor WebAssembly applications.</summary>
public interface IAssetService
{
    /// <summary>Returns the upload limits and current file usage for the authenticated user.</summary>
    Task<ResultResponse<GetUserUploadLimitsResponse>> GetUserUploadLimitsAsync();

    /// <summary>Creates a new asset metadata record. Returns the assigned ID on success, or a <see cref="FailureResponse"/> on failure.</summary>
    Task<ResultResponse<CreateAssetResponse>> CreateAssetAsync(CreateAssetRequest request);

    /// <summary>Returns a paged list of asset metadata for administrators. Optionally filters by asset ID or user ID.</summary>
    Task<ResultResponse<GetAssetsMetadataResponse>> GetAssetsMetadataAsync(int page, Guid? assetId = null, Guid? userId = null);

    /// <summary>Returns a paged list of asset metadata for the authenticated user.</summary>
    Task<ResultResponse<GetAssetsMetadataResponse>> GetMyAssetsMetadataAsync(int page);

    /// <summary>Returns the metadata of an asset by ID.</summary>
    Task<ResultResponse<GetAssetMetadataResponse>> GetAssetMetadataAsync(Guid id);

    /// <summary>Uploads binary content for an existing asset. Returns the asset ID on success, or a <see cref="FailureResponse"/> on failure.</summary>
    Task<ResultResponse<UploadAssetContentResponse>> UploadAssetContentAsync(Guid id, Stream content, string fileName, string contentType);

    /// <summary>Deletes the content of an asset by ID. Returns a <see cref="FailureResponse"/> on failure.</summary>
    Task<ResultResponse<EmptyResponse>> DeleteAssetContentAsync(Guid id);

    /// <summary>Deletes the content of the authenticated user's own asset by ID. Returns a <see cref="FailureResponse"/> on failure.</summary>
    Task<ResultResponse<EmptyResponse>> DeleteMyAssetContentAsync(Guid id);

    /// <summary>Returns the direct URL to download the binary content of an asset. The browser streams it natively — no in-memory buffering.</summary>
    string GetContentUrl(Guid id);
}