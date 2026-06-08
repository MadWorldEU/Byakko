namespace MadWorldEU.Byakko.Storages;

/// <summary>
/// Response containing storage statistics across all active assets.
/// </summary>
public sealed class GetStorageStatisticsResponse
{
    /// <summary>
    /// Total number of files currently saved in storage.
    /// </summary>
    public required int TotalFiles { get; init; }

    /// <summary>
    /// Total size in bytes of all files currently saved in storage.
    /// </summary>
    public required long TotalBytes { get; init; }
}