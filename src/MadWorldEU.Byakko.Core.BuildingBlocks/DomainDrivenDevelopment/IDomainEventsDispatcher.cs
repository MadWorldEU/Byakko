namespace MadWorldEU.Byakko.DomainDrivenDevelopment;

/// <summary>Dispatches domain events to their registered handlers.</summary>
public interface IDomainEventsDispatcher
{
    /// <summary>Dispatches each event in the collection to all matching handlers.</summary>
    Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default);
}