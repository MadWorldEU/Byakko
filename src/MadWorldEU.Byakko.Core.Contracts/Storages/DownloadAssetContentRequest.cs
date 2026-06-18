namespace MadWorldEU.Byakko.Storages;

/// <summary>Request to download the binary content of an asset, with an optional password for encrypted files.</summary>
public sealed class DownloadAssetContentRequest
{
    /// <summary>The password used to decrypt the file, or <see langword="null"/> if the file is not password-protected.</summary>
    public string? Password { get; init; }
}