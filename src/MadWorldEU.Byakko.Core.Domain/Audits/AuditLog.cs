using MadWorldEU.Byakko.Systems;

namespace MadWorldEU.Byakko.Audits;

public sealed class AuditLog : Entity<Id>
{
    public AuditEntityType EntityType { get; }
    public Id EntityId { get; }
    public IpAddress IpAddress { get; }
    public Instant OccurredAt { get; }
    public UserId OccurredBy { get; }

    private AuditLog(Id id, Id entityId, IpAddress ipAddress, Instant occurredAt, UserId occurredBy)
    {
        Id = id;
        EntityType = AuditEntityType.Asset;
        EntityId = entityId;
        IpAddress = ipAddress;
        OccurredAt = occurredAt;
        OccurredBy = occurredBy;
    }
    
    public static Result<AuditLog> Create(        
        IClock clock,
        IGuidGenerator guidGenerator,
        Id entityId,
        IpAddress ipAddress,
        UserId userId)
    {
        var now = clock.GetCurrentInstant();
        var id = Id.Create(guidGenerator.New()).Value;
        return new AuditLog(id, entityId, ipAddress, now, userId);
    }
}