namespace MadWorldEU.Byakko.Accounts;

public sealed class ConfirmDeletionAccountUseCase(IClock clock, IAccountRepository accountRepository)
{
    public async Task<Result<ConfirmDeletionAccountResponse>> ExecuteAsync(string userId)
    {
        var userIdResult = UserId.Create(userId);
        if (userIdResult.IsFailure) return userIdResult.Error;
        
        var accountResult = await accountRepository.FindAsync(userIdResult.Value);
        if (accountResult.IsFailure) return accountResult.Error;
        var account = accountResult.Value;
        
        var requestResult = account.ConfirmDeletion(clock);
        if (requestResult.IsFailure) return requestResult.Error;
        
        var saveResult = await accountRepository.UpdateAsync(account);
        if (saveResult.IsFailure) return saveResult.Error;
        
        return new ConfirmDeletionAccountResponse
        {
            UserId = Guid.Empty
        };
    }
}