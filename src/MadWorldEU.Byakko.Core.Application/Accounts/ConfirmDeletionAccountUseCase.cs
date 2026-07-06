namespace MadWorldEU.Byakko.Accounts;

/// <summary>Confirms a pending GDPR deletion request for the given user account.</summary>
public sealed class ConfirmDeletionAccountUseCase(IClock clock, IAccountRepository accountRepository, ILogger<ConfirmDeletionAccountUseCase> logger)
{
    /// <summary>Executes the confirmation for the given Keycloak user ID.</summary>
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
        
        logger.LogInformation("Account '{UserId}' deletion request confirmed.", account.UserId.Value);
        
        return new ConfirmDeletionAccountResponse
        {
            UserId = account.UserId.Value
        };
    }
}