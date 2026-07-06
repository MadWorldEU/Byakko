using MadWorldEU.Byakko.Common.Pages;
using MadWorldEU.Byakko.Functional;
using Microsoft.Extensions.Logging;

namespace MadWorldEU.Byakko.Accounts;

/// <summary>PostgreSQL implementation of <see cref="IAccountRepository"/> backed by <see cref="ByakkoContext"/>.</summary>
public sealed class AccountRepository(ByakkoContext context, ILogger<AccountRepository> logger) : IAccountRepository
{
    /// <inheritdoc />
    public async Task<Result<Account>> FindAsync(UserId userId)
    {
        Account? account;

        try
        {
            account = await context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to query account for user '{UserId}'.", userId.Value);
            return AccountErrors.QueryFailed;
        }

        if (account is null)
        {
            logger.LogInformation("Account for user '{UserId}' not found.", userId.Value);
            return AccountErrors.NotFound;
        }

        return account;
    }

    /// <inheritdoc />
    public async Task<Result> AddAsync(Account account)
    {
        try
        {
            await context.Accounts.AddAsync(account);
            await context.SaveChangesAsync();

            logger.LogInformation("Account '{AccountId}' added successfully.", account.Id.Value);
            return Result.Success();
        }
        catch (DbUpdateException exception)
        {
            logger.LogError(exception, "Failed to save account '{AccountId}'.", account.Id.Value);
            return Result.Failure(AccountErrors.SaveFailed);
        }
    }

    /// <inheritdoc />
    public async Task<Result> UpdateAsync(Account account)
    {
        try
        {
            context.Accounts.Update(account);
            await context.SaveChangesAsync();

            logger.LogInformation("Account '{AccountId}' updated successfully.", account.Id.Value);
            return Result.Success();
        }
        catch (DbUpdateException exception)
        {
            logger.LogError(exception, "Failed to update account '{AccountId}'.", account.Id.Value);
            return Result.Failure(AccountErrors.UpdateFailed);
        }
    }

    public async Task<Result<PagedResult<Account>>> GetDeleteRequestedAccounts(Page page)
    {
        var pageSize = PageSize.Create(20).Value;
        
        try
        {
            var query = context.Accounts
                .AsQueryable()
                .Where(a => a.Status == AccountStatus.DeletionRequested);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(a => a.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Account>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to query paged accounts.");
            return AccountErrors.QueryFailed;
        }
    }
}