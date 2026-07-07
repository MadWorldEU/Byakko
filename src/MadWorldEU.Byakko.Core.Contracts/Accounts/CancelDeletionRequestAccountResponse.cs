namespace MadWorldEU.Byakko.Accounts;

/// <summary>
/// Response returned after successfully cancelling a deletion request for an account.
/// </summary>
public sealed class CancelDeletionRequestAccountResponse
{
    /// <summary>
    /// The unique identifier of the account whose deletion request was cancelled.
    /// </summary>
    public required Guid UserId { get; init; }
}