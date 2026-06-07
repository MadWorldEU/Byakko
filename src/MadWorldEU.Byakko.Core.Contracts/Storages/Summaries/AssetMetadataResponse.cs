namespace MadWorldEU.Byakko.Storages.Summaries;

/// <summary>Metadata for a single asset within a paged response.</summary>
public sealed class AssetMetadataResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string ContentType { get; init; }
    public required Guid UserId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required bool IsDeleted { get; init; }
    public required long Size { get; init; }
}