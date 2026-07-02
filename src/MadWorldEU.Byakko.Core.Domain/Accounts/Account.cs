using MadWorldEU.Byakko.Systems;

namespace MadWorldEU.Byakko.Accounts;

/// <summary>Represents a registered user account, tracking identity and GDPR deletion state.</summary>
public sealed class Account : Entity<Id>
{
    public UserId UserId { get; private set; } = null!;
    public bool HasDeletionRequested { get; private set; }
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
        CreatedAt = CreatedAt;
        UpdatedAt = CreatedAt;
    }
    
    /// <summary>Creates a new account for the given user, stamped with the current time.</summary>
    public static Result<Account> Create(IClock clock, IGuidGenerator guidGenerator, UserId userId)
    {
        var now = clock.GetCurrentInstant();
        var id = Id.Create(guidGenerator.New()).Value;
        return new Account(id, userId, now);
    }
    
    /// <summary>Flags the account for deletion. Returns a failure if a request was already made.</summary>
    public Result RequestDeletion(IClock clock)
    {
        if (HasDeletionRequested)
        {
            return Result.Failure(AccountErrors.DeletionAlreadyRequested);
        }
        
        UpdatedAt = clock.GetCurrentInstant();
        HasDeletionRequested = true;
        
        return Result.Success();
    }
}