namespace MadWorldEU.Byakko.Storages;

/// <summary>Application-level asset configuration bound from the <c>Assets</c> appsettings section.</summary>
public sealed class AssetSettings
{
    public const string Key = "Assets";

    /// <summary>Number of days a newly created asset remains valid before it expires.</summary>
    public int ValidityPeriodInDays { get; init; }
}