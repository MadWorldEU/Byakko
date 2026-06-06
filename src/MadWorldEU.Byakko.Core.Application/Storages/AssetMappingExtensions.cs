using MadWorldEU.Byakko.Storages.Summaries;

namespace MadWorldEU.Byakko.Storages;

/// <summary>Mapping extensions for the Asset domain entity.</summary>
internal static class AssetMappingExtensions
{
    internal static AssetMetadataResponse ToMetadataResponse(this Asset asset) =>
        new()
        {
            Id = asset.Id.Value,
            Name = asset.Name.Value,
            ContentType = asset.ContentType.Value,
            UserId = asset.CreatedBy.Value,
            CreatedAt = asset.CreatedAt.ToDateTimeOffset(),
            UpdatedAt = asset.UpdatedAt.ToDateTimeOffset(),
            ExpiresAt = asset.ExpiresAt.ToDateTimeOffset(),
        };
}