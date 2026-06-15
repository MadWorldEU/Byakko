namespace MadWorldEU.Byakko.DomainDrivenDevelopment;

/// <summary>Handles a specific type of domain event.</summary>
public interface IDomainEventHandler<in T> where T : IDomainEvent
{
    /// <summary>Processes the domain event.</summary>
    Task Handle(T domainEvent, CancellationToken cancellationToken = default);
}
