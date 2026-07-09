using MadWorldEU.Byakko.Accounts;
using MadWorldEU.Byakko.DomainDrivenDevelopment;

namespace MadWorldEU.Byakko.Audits;

public sealed class AuditAccountEventHandler(IAuditRepository auditRepository) : IDomainEventHandler<AccountDeletedEvent>
{
    public async Task Handle(AccountDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await auditRepository.DeleteAsync(domainEvent.UserId);
    }
}