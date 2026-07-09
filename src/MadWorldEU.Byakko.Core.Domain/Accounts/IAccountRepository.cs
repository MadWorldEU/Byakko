using MadWorldEU.Byakko.Common.Pages;

namespace MadWorldEU.Byakko.Accounts;

/// <summary>Persistence contract for <see cref="Account"/> aggregates.</summary>
public interface IAccountRepository
{
    /// <summary>Finds the account associated with the given user.</summary>
    Task<Result<Account>> FindAsync(UserId userId);

    /// <summary>Persists a new account to the database.</summary>
    Task<Result> AddAsync(Account account);

    /// <summary>Returns a paged list of accounts that have a pending deletion request.</summary>
    Task<Result<PagedResult<Account>>> GetAccountsPendingDeletion(Page page);
    
    /// <summary>Returns all accounts whose deletion has been confirmed and are ready to be permanently deleted.</summary>
    Task<Result<List<Account>>> GetConfirmedDeletionAccounts();
    
    /// <summary>Persists changes to an existing account.</summary>
    Task<Result> UpdateAsync(Account account);
}