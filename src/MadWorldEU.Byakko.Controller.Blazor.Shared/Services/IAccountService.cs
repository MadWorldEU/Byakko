using MadWorldEU.Byakko.Accounts;

namespace MadWorldEU.Byakko.Services;

/// <summary>Wraps the Accounts API endpoints for use in Blazor WebAssembly applications.</summary>
public interface IAccountService
{
    /// <summary>Returns a paged list of accounts with a pending deletion request.</summary>
    Task<ResultResponse<GetDeleteRequestedAccountsResponse>> GetDeleteRequestedAccountsAsync(int page);

    /// <summary>Creates an account for the currently authenticated user.</summary>
    Task<ResultResponse<CreateMyAccountResponse>> CreateMyAccountAsync();

    /// <summary>Returns the account details of the currently authenticated user.</summary>
    Task<ResultResponse<GetMyAccountResponse>> GetMyAccountAsync();

    /// <summary>Submits a GDPR deletion request for the currently authenticated user's account.</summary>
    Task<ResultResponse<RequestDeletionMyAccountResponse>> RequestDeletionMyAccountAsync();
}