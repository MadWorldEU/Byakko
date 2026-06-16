using NodaTime;

namespace MadWorldEU.Byakko.DomainDrivenDevelopment;

/// <summary>Marker interface for domain events raised by aggregates.</summary>
public interface IDomainEvent
{
    Instant OccurredOn { get; }
}