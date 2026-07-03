namespace MadWorldEU.Byakko.Accounts;

/// <summary>Submits a GDPR deletion request for the authenticated user's account.</summary>
public sealed class RequestDeletionMyAccountUseCase(IClock clock, IAccountRepository accountRepository)
{
    /// <summary>Executes the deletion request for the given Keycloak user ID.</summary>
    public async Task<Result<RequestDeletionMyAccountResponse>> ExecuteAsync(string userId)
    {
        var userIdResult = UserId.Create(userId);
        if (userIdResult.IsFailure) return userIdResult.Error;
        
        var accountResult = await accountRepository.FindAsync(userIdResult.Value);
        if (accountResult.IsFailure) return accountResult.Error;
        var account = accountResult.Value;
        
        var requestResult = account.RequestDeletion(clock);
        if (requestResult.IsFailure) return requestResult.Error;

        var saveResult = await accountRepository.UpdateAsync(accountResult.Value);
        if (saveResult.IsFailure) return saveResult.Error;
        
        return new RequestDeletionMyAccountResponse()
        {
            UserId = account.UserId.Value,
        };
    }

}