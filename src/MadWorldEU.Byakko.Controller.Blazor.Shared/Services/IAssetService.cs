using MadWorldEU.Byakko.Storages;

namespace MadWorldEU.Byakko.Services;

/// <summary>Wraps the Assets API endpoints for use in Blazor WebAssembly applications.</summary>
public interface IAssetService
{
    /// <summary>Creates a new asset metadata record and returns the assigned ID.</summary>
    Task<CreateAssetResponse?> CreateAssetAsync(CreateAssetRequest request);

    /// <summary>Returns the metadata of an asset by ID.</summary>
    Task<GetAssetMetadataResponse?> GetAssetMetadataAsync(Guid id);

    /// <summary>Uploads binary content for an existing asset.</summary>
    Task<UploadAssetContentResponse?> UploadAssetContentAsync(Guid id, Stream content, string fileName, string contentType);

    /// <summary>Downloads the binary content of an asset and returns it as a byte array together with its content type and file name.</summary>
    Task<(byte[] Content, string ContentType, string FileName)?> DownloadAssetContentAsync(Guid id);
}