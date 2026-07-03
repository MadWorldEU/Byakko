namespace MadWorldEU.Byakko.Accounts.Summaries;

public class AccountResponse
{
    public required Guid UserId { get; init; }
    public required bool HasDeletionRequested { get; init; }
}