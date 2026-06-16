using MadWorldEU.Byakko.Audits;

namespace MadWorldEU.Byakko.Storages;

public record AssetContentUploadedEvent( 
    Id AssetId, 
    IpAddress IpAddress, 
    UserId CreatedBy, 
    Instant OccurredOn) : IDomainEvent;