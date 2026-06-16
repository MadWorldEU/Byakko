using MadWorldEU.Byakko.Audits;

namespace MadWorldEU.Byakko.Storages;

public sealed record AssetMetaDataCreatedEvent( 
    Id AssetId, 
    IpAddress IpAddress, 
    UserId CreatedBy, 
    Instant OccurredOn) : IDomainEvent;