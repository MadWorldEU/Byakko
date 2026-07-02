namespace MadWorldEU.Byakko.Accounts;

/// <summary>Response returned after successfully submitting an account deletion request.</summary>
public sealed class RequestDeletionMyAccountResponse
{
    public required Guid UserId { get; init; }
}