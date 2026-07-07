namespace MadWorldEU.Byakko.Accounts;

/// <summary>
/// Cancels a pending deletion request for a user account, restoring it to active status.
/// </summary>
public sealed class CancelDeletionRequestAccountUseCase(IClock clock, IAccountRepository accountRepository, ILogger<CancelDeletionRequestAccountUseCase> logger)
{
    /// <summary>
    /// Cancels the deletion request for the account identified by <paramref name="userId"/>.
    /// Returns <see cref="AccountErrors.NotFound"/> when the account does not exist,
    /// or <see cref="AccountErrors.DeletionNotRequested"/> when the account has no active deletion request.
    /// </summary>
    public async Task<Result<CancelDeletionRequestAccountResponse>> ExecuteAsync(string userId)
    {
        var userIdResult = UserId.Create(userId);
        if (userIdResult.IsFailure) return userIdResult.Error;
        
        var accountResult = await accountRepository.FindAsync(userIdResult.Value);
        if (accountResult.IsFailure) return accountResult.Error;
        var account = accountResult.Value;
        
        var requestResult = account.CancelDeletionRequest(clock);
        if (requestResult.IsFailure) return requestResult.Error;
        
        var saveResult = await accountRepository.UpdateAsync(account);
        if (saveResult.IsFailure) return saveResult.Error;
        
        logger.LogInformation("Account '{UserId}' deletion request cancelled.", account.UserId.Value);       
        
        return new CancelDeletionRequestAccountResponse()
        {
            UserId = account.UserId.Value
        };
    }
}