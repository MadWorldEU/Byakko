using MadWorldEU.Byakko.Accounts.Summaries;

namespace MadWorldEU.Byakko.Accounts;

/// <summary>Returns a paged list of accounts that have submitted a GDPR deletion request.</summary>
public sealed class GetAccountsPendingDeletionUseCase(IAccountRepository accountRepository)
{
    /// <summary>Queries accounts with a pending deletion request for the given page.</summary>
    public async Task<Result<GetAccountsPendingDeletionResponse>> QueryAsync(int page)
    {
        var pageResult = Page.Create(page);
        if (pageResult.IsFailure) return pageResult.Error;
        
        var accountsResult = await accountRepository.GetAccountsPendingDeletion(pageResult.Value);
        if (accountsResult.IsFailure) return accountsResult.Error;

        return new GetAccountsPendingDeletionResponse()
        {
            Accounts = accountsResult.Value.Items
                .Select(i => new AccountResponse()
                {
                    UserId = i.UserId.Value,
                    Status = i.Status.ToString(),
                    UpdatedAt = i.UpdatedAt.ToDateTimeOffset()
                }).ToList(),
            Page = accountsResult.Value.Page,
            PageSize = accountsResult.Value.PageSize,
            TotalCount = accountsResult.Value.TotalCount,
            HasNextPage = accountsResult.Value.HasNextPage
        };
    }
}