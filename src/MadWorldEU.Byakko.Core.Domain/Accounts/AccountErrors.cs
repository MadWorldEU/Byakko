namespace MadWorldEU.Byakko.Accounts;

/// <summary>Domain errors for the account.</summary>
public static class AccountErrors
{
    public static readonly Error NotActive = Error.Create("Account.NotActive", "The account is not active.");
    public static readonly Error DeletionAlreadyRequested = Error.Create("Account.DeletionAlreadyRequested", "A deletion request has already been made for this account.");
    public static readonly Error DeletionNotRequested = Error.Create("Account.DeletionNotRequested", "No deletion request has been made for this account.");
    public static readonly Error DeletionNotConfirmed = Error.Create("Account.DeletionNotConfirmed", "The deletion request has not been confirmed yet.");
    public static readonly Error NotFound = Error.Create("Account.NotFound", "The account could not be found.");
    public static readonly Error QueryFailed = Error.Create("Account.QueryFailed", "The account could not be queried.");
    public static readonly Error SaveFailed = Error.Create("Account.SaveFailed", "The account could not be saved.");
    public static readonly Error UpdateFailed = Error.Create("Account.UpdateFailed", "The account could not be updated.");
}