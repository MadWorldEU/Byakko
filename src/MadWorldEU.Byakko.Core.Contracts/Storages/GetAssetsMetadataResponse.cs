using MadWorldEU.Byakko.Storages.Summaries;

namespace MadWorldEU.Byakko.Storages;

/// <summary>Response containing a paged list of asset metadata.</summary>
public sealed class GetAssetsMetadataResponse
{
    public required IReadOnlyList<AssetMetadataResponse> Assets { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    public required bool HasNextPage { get; init; }
}