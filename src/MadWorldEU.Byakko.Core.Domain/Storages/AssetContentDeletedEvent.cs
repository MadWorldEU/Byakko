using MadWorldEU.Byakko.Audits;

namespace MadWorldEU.Byakko.Storages;

public record AssetContentDeletedEvent( 
    Id AssetId, 
    IpAddress IpAddress, 
    UserId CreatedBy, 
    Instant OccurredOn) : IDomainEvent;