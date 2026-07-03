namespace MadWorldEU.Byakko.Accounts;

/// <summary>Response containing the authenticated user's account details.</summary>
public sealed class GetMyAccountResponse
{
    public required Guid UserId { get; init; }
    public required bool HasDeletionRequested { get; init; }
}