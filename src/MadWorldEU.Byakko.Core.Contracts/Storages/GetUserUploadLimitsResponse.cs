namespace MadWorldEU.Byakko.Storages;

/// <summary>Response containing the upload limits and current usage for a user.</summary>
public sealed class GetUserUploadLimitsResponse
{
    /// <summary>Maximum number of active files the user is allowed to have at one time.</summary>
    public required int MaxFiles { get; init; }

    /// <summary>Maximum allowed upload size in bytes per file.</summary>
    public required long MaxUploadSizeInBytes { get; init; }

    /// <summary>Current number of active (non-deleted, non-expired) files owned by the user.</summary>
    public required int ActiveFiles { get; init; }
}