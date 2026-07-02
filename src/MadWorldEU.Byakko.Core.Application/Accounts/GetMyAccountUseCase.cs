namespace MadWorldEU.Byakko.Accounts;

/// <summary>Retrieves the account details for the authenticated user.</summary>
public sealed class GetMyAccountUseCase(IAccountRepository accountRepository)
{
    /// <summary>Queries the account for the given Keycloak user ID.</summary>
    public async Task<Result<GetMyAccountResponse>> QueryAsync(string userId)
    {
        var userIdResult = UserId.Create(userId);
        if (userIdResult.IsFailure) return userIdResult.Error;
        
        var accountResult = await accountRepository.FindAsync(userIdResult.Value);
        if (accountResult.IsFailure) return accountResult.Error;
        
        return new GetMyAccountResponse()
        {
            UserId = accountResult.Value.UserId.Value,
            HasDeletionRequested = accountResult.Value.HasDeletionRequested
        };
    }
}