using MadWorldEU.Byakko.Accounts;
using MadWorldEU.Byakko.DomainDrivenDevelopment;

namespace MadWorldEU.Byakko.AuthenticationServers;

public class AuthenticationServerAccountEventHandler(IAuthenticationRepository authenticationRepository) : IDomainEventHandler<AccountDeletedEvent>
{
    public Task Handle(AccountDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        return authenticationRepository.DeleteUser(domainEvent.UserId);
    }
}