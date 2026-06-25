namespace MadWorldEU.Byakko.Storages;

/// <summary>Response containing the metadata of an asset.</summary>
public sealed class GetAssetMetadataResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string ContentType { get; init; }
    public required long Size { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
}