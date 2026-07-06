namespace MadWorldEU.Byakko.Accounts.Summaries;

public class AccountResponse
{
    public required Guid UserId { get; init; }
    public required string Status { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
}