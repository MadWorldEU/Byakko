namespace MadWorldEU.Byakko.Audits.Summaries;

/// <summary>A single audit log entry describing an action performed on a domain entity.</summary>
public sealed class AuditLogResponse
{
    /// <summary>Unique identifier of the audit log entry.</summary>
    public required Guid Id { get; init; }

    /// <summary>The type of entity the action was performed on.</summary>
    public required string EntityType { get; init; }

    /// <summary>The action that was performed.</summary>
    public required string Action { get; init; }

    /// <summary>The IP address from which the action originated.</summary>
    public required string IpAddress { get; init; }

    /// <summary>The UTC instant at which the action occurred.</summary>
    public required DateTimeOffset OccurredAt { get; init; }

    /// <summary>The ID of the user who performed the action.</summary>
    public required Guid OccurredByUserId { get; init; }
}