namespace MadWorldEU.Byakko.Storages;

/// <summary>Application-level asset configuration bound from the <c>Assets</c> appsettings section.</summary>
public sealed class AssetSettings
{
    public const string Key = "Assets";

    /// <summary>Number of days a newly created asset remains valid before it expires.</summary>
    public int ValidityPeriodInDays { get; init; }

    /// <summary>Maximum allowed upload size in bytes. Enforced by both the API and the Portal UI.</summary>
    public long MaxUploadSizeInBytes { get; init; }

    /// <summary>Maximum number of active (non-deleted, non-expired) files a single user may have at one time.</summary>
    public int MaxFilesEachUser { get; init; }
}