using MadWorldEU.Byakko.Accounts;
using MadWorldEU.Byakko.DomainDrivenDevelopment;

namespace MadWorldEU.Byakko.Audits;

/// <summary>Deletes all audit log entries for a user when their account has been permanently deleted.</summary>
public sealed class AuditAccountEventHandler(IAuditRepository auditRepository) : IDomainEventHandler<AccountDeletedEvent>
{
    /// <summary>Handles the <see cref="AccountDeletedEvent"/> by removing the user's audit log entries.</summary>
    public Task Handle(AccountDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        return auditRepository.DeleteAsync(domainEvent.UserId);
    }
}