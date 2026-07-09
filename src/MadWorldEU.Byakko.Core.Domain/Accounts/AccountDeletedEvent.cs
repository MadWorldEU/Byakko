namespace MadWorldEU.Byakko.Accounts;

public record AccountDeletedEvent(UserId UserId, Instant OccurredOn) : IDomainEvent;