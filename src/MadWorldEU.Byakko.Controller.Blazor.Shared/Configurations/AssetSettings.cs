namespace MadWorldEU.Byakko.Configurations;

/// <summary>Asset configuration for Blazor clients, bound from the <c>Assets</c> appsettings section.</summary>
public sealed class AssetSettings
{
    public const string Key = "Assets";

    /// <summary>Maximum allowed upload size in bytes enforced by the client before sending the request.</summary>
    public long MaxUploadSizeInBytes { get; init; } = 1_073_741_824;

    /// <summary>Maximum number of days a newly created asset may remain valid. Enforced by the API; used by the Portal to cap the expiry date picker.</summary>
    public int MaxValidityPeriodInDays { get; init; } = 30;
}