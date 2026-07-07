using MadWorldEU.Byakko.Systems;

namespace MadWorldEU.Byakko.Accounts;

/// <summary>Represents a registered user account, tracking identity and GDPR deletion state.</summary>
public sealed class Account : Entity<Id>
{
    public UserId UserId { get; private set; } = null!;
    public AccountStatus Status { get; private set; }
    public Instant CreatedAt { get; private init; }
    public Instant UpdatedAt { get; private set; }

    /// <summary>
    /// Required for EF Core
    /// </summary>
    [UsedImplicitly]
    private Account() {}

    private Account(Id id, UserId userId, Instant createdAt)
    {
        Id = id;
        UserId = userId;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    /// <summary>Creates a new account for the given user, stamped with the current time.</summary>
    public static Result<Account> Create(IClock clock, IGuidGenerator guidGenerator, UserId userId)
    {
        var now = clock.GetCurrentInstant();
        var id = Id.Create(guidGenerator.New()).Value;
        return new Account(id, userId, now);
    }

    /// <summary>Flags an account for deletion. Returns a failure if a request was already made.</summary>
    public Result RequestDeletion(IClock clock)
    {
        if (Status != AccountStatus.Active)
        {
            return Result.Failure(AccountErrors.NotActive);
        }

        UpdatedAt = clock.GetCurrentInstant();
        Status = AccountStatus.DeletionRequested;

        return Result.Success();
    }

    /// <summary>
    /// Cancels a pending deletion request, restoring the account to <see cref="AccountStatus.Active"/>.
    /// Returns <see cref="AccountErrors.DeletionNotRequested"/> if the account is not in <see cref="AccountStatus.DeletionRequested"/> status.
    /// </summary>
    public Result CancelDeletionRequest(IClock clock)
    {
        if (Status != AccountStatus.DeletionRequested)
        {
            return Result.Failure(AccountErrors.DeletionNotRequested);
        }
        
        UpdatedAt = clock.GetCurrentInstant();
        Status = AccountStatus.Active;

        return Result.Success();
    }

    /// <summary>Marks the deletion as confirmed by an administrator. Returns a failure if no deletion request is pending.</summary>
    public Result ConfirmDeletion(IClock clock)
    {
        if (Status != AccountStatus.DeletionRequested)
        {
            return Result.Failure(AccountErrors.DeletionNotRequested);
        }

        UpdatedAt = clock.GetCurrentInstant();
        Status = AccountStatus.DeletionConfirmed;

        return Result.Success();
    }
}