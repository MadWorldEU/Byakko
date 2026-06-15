namespace MadWorldEU.Byakko.Audits;

/// <summary>Repository for persisting audit log entries.</summary>
public interface IAuditRepository
{
    /// <summary>Persists a new audit log entry.</summary>
    Task<Result> AddAsync(AuditLog auditLog);
}