namespace MadWorldEU.Byakko.Audits;

/// <summary>Repository for persisting audit log entries.</summary>
public interface IAuditRepository
{
    /// <summary>Persists a new audit log entry.</summary>
    Task<Result> AddAsync(AuditLog auditLog);

    /// <summary>Deletes all audit log entries associated with the given user.</summary>
    Task<Result> DeleteAsync(UserId userId);

    /// <summary>Retrieves all audit log entries for the given entity.</summary>
    Task<Result<IReadOnlyList<AuditLog>>> GetAsync(Id entityId);
}