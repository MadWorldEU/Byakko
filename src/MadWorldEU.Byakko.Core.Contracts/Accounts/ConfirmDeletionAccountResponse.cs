namespace MadWorldEU.Byakko.Accounts;

/// <summary>Response returned after an administrator confirms an account deletion request.</summary>
public sealed class ConfirmDeletionAccountResponse
{
    public required Guid UserId { get; init; }
}