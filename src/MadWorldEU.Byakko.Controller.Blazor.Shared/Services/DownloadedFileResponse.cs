namespace MadWorldEU.Byakko.Services;

/// <summary>Holds the binary content and metadata of a downloaded asset, ready for browser delivery.</summary>
public sealed class DownloadedFileResponse
{
    /// <summary>The raw file bytes.</summary>
    public required byte[] Bytes { get; init; }

    /// <summary>The MIME content type (e.g. <c>image/png</c>).</summary>
    public required string ContentType { get; init; }

    /// <summary>The suggested filename for the browser download prompt.</summary>
    public required string FileName { get; init; }
}