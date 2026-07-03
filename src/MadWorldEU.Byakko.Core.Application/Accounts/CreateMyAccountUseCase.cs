using MadWorldEU.Byakko.Systems;

namespace MadWorldEU.Byakko.Accounts;

/// <summary>Creates a new account for the authenticated user.</summary>
public sealed class CreateMyAccountUseCase(
    IClock clock, 
    IGuidGenerator guidGenerator, 
    IAccountRepository accountRepository,
    ILogger<CreateMyAccountUseCase> logger)
{
    /// <summary>Executes the use case for the given Keycloak user ID.</summary>
    public async Task<Result<CreateMyAccountResponse>> ExecuteAsync(string userId)
    {
        var userIdResult = UserId.Create(userId);
        if (userIdResult.IsFailure) return userIdResult.Error;
        
        var accountResult = Account.Create(clock, guidGenerator, userIdResult.Value);
        if (accountResult.IsFailure) return accountResult.Error;
        
        var saveResult = await accountRepository.AddAsync(accountResult.Value);
        if (saveResult.IsFailure) return saveResult.Error;
        
        logger.LogInformation("Account '{UserId}' created.", accountResult.Value.UserId.Value);
        
        return new CreateMyAccountResponse()
        {
            UserId = accountResult.Value.UserId.Value
        };
    }
}