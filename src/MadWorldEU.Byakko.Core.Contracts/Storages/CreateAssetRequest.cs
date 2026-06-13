namespace MadWorldEU.Byakko.Storages;

/// <summary>Request to create a new asset metadata record.</summary>
public sealed class CreateAssetRequest
{
    public string Name { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Size { get; init; }
}