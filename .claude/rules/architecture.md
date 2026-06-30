# Architecture

## DDD Building Blocks

`Core.BuildingBlocks/DomainDrivenDevelopment/`, namespace `MadWorldEU.Byakko.DomainDrivenDevelopment`. Register via `services.AddBuildingBlocks()`. Call `IDomainEventsDispatcher.DispatchAsync(aggregate)` after a use case completes.

| Type | Usage |
|---|---|
| `AggregateRoot<TId>` | Root aggregate; `RaiseDomainEvent` / `ClearDomainEvents` |
| `Entity<TId>` | Identity equality by `Id` |
| `ValueObject` | Immutable; structural equality via `GetEqualityComponents()` |
| `IDomainEvent` | Marker interface |
| `IDomainEventHandler<T>` | One class per event type |
| `IDomainEventsDispatcher` | Resolves handlers from DI; cached `HandlerWrapper` pattern |
| `IGuidGenerator` | Abstracts `Guid.NewGuid()`; inject into use cases |

## Functional Patterns

`Core.BuildingBlocks/Functional/`, namespace `MadWorldEU.Byakko.Functional`. `Error.Create("Domain.Reason", "description")`. `Result.Success()` / `Result.Failure(error)`. `Result<T>` value implicitly converts to success. Use `result.Match(onSuccess, onFailure)`.
