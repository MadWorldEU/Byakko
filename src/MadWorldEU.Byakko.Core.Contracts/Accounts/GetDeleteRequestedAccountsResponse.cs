using MadWorldEU.Byakko.Accounts.Summaries;

namespace MadWorldEU.Byakko.Accounts;

/// <summary>Paged response containing accounts that have submitted a deletion request.</summary>
public sealed class GetDeleteRequestedAccountsResponse
{
    public required List<AccountResponse> Accounts { get; init; } = [];
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    public required bool HasNextPage { get; init; }
}