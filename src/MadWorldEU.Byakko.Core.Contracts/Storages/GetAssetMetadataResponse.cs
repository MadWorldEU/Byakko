namespace MadWorldEU.Byakko.Storages;

public sealed class GetAssetMetadataResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string ContentType { get; init; }
    public required string CreatedAt { get; init; }
}