namespace MadWorldEU.Byakko.Common.Pages;

/// <summary>Wraps a page of items alongside pagination metadata.</summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public required int TotalCount { get; init; }
    public required Page Page { get; init; }
    public required PageSize PageSize { get; init; }

    public bool HasNextPage => Page * PageSize < TotalCount;
}