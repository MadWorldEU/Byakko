using MadWorldEU.Byakko.Systems;

namespace MadWorldEU.Byakko.Audits;

/// <summary>An immutable record of an auditable action performed on a domain entity.</summary>
public sealed class AuditLog : Entity<Id>
{
    /// <summary>The type of entity the action was performed on.</summary>
    public AuditEntityType EntityType { get; }

    /// <summary>The identifier of the entity the action was performed on.</summary>
    public Id EntityId { get; }
    
    public AuditAction Action { get; }

    /// <summary>The IP address from which the action originated.</summary>
    public IpAddress IpAddress { get; }

    /// <summary>The UTC instant at which the action occurred.</summary>
    public Instant OccurredAt { get; }

    /// <summary>The user who performed the action.</summary>
    public UserId OccurredBy { get; }

    private AuditLog(Id id, Id entityId, AuditAction action, IpAddress ipAddress, Instant occurredAt, UserId occurredBy)
    {
        Id = id;
        EntityType = AuditEntityType.Asset;
        EntityId = entityId;
        Action = action;
        IpAddress = ipAddress;
        OccurredAt = occurredAt;
        OccurredBy = occurredBy;
    }
    
    /// <summary>Creates a new audit log entry stamped with the current time.</summary>
    public static Result<AuditLog> Create(
        IClock clock,
        IGuidGenerator guidGenerator,
        Id entityId,
        AuditAction action,
        IpAddress ipAddress,
        UserId userId)
    {
        var now = clock.GetCurrentInstant();
        var id = Id.Create(guidGenerator.New()).Value;
        return new AuditLog(id, entityId, action, ipAddress, now, userId);
    }
}