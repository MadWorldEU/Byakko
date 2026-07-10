using MadWorldEU.Byakko.DomainDrivenDevelopment;

namespace MadWorldEU.Byakko.Accounts;

/// <summary>Fetches all deletion-confirmed accounts and permanently marks them as deleted, dispatching a domain event per account.</summary>
public sealed class DeleteRequestedAccountsUseCase(
    IClock clock, 
    IAccountRepository accountRepository, 
    IDomainEventsDispatcher domainEventsDispatcher,
    ILogger<DeleteRequestedAccountsUseCase> logger)
{
    public async Task<Result> ExecuteAsync()
    {
        var accountsResult = await accountRepository.GetConfirmedDeletionAccounts();
        if (accountsResult.IsFailure) return Result.Failure(accountsResult.Error);

        foreach (var account in accountsResult.Value)
        {
            var deleteResult = account.Delete(clock);

            if (deleteResult.IsFailure)
            {
                logger.LogWarning("Account {AccountId} deletion failed: {Error}", account.Id, deleteResult.Error);
                continue;
            }
            
            await accountRepository.UpdateAsync(account);
            logger.LogInformation("Account {AccountId} deleted", account.Id);

            var accountDeletedEvent = new AccountDeletedEvent(account.UserId, account.UpdatedAt);
            await domainEventsDispatcher.DispatchAsync([accountDeletedEvent]);
        }
        
        return Result.Success();
    }
}