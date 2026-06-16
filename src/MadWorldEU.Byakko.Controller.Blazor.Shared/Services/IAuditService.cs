using MadWorldEU.Byakko.Audits;

namespace MadWorldEU.Byakko.Services;

/// <summary>Wraps the Audits API endpoints for use in Blazor WebAssembly applications.</summary>
public interface IAuditService
{
    /// <summary>Returns all audit log entries for the given entity ID.</summary>
    Task<GetAuditLogsResponse?> GetAuditLogsAsync(Guid entityId);
}