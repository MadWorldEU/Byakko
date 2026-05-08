namespace MadWorldEU.Byakko.DomainDrivenDevelopment;

/// <summary>Marker interface for domain events raised by aggregates.</summary>
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}