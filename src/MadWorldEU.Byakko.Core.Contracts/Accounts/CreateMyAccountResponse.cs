namespace MadWorldEU.Byakko.Accounts;

/// <summary>Response returned after successfully creating an account.</summary>
public sealed class CreateMyAccountResponse
{
    public required Guid UserId { get; init; }
}