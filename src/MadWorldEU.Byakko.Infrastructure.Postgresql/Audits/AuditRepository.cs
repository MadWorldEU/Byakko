using MadWorldEU.Byakko.Functional;
using Microsoft.Extensions.Logging;

namespace MadWorldEU.Byakko.Audits;

/// <summary>PostgreSQL implementation of <see cref="IAuditRepository"/>.</summary>
public sealed class AuditRepository(ByakkoContext context, ILogger<AuditRepository> logger) : IAuditRepository
{
    /// <inheritdoc />
    public async Task<Result> AddAsync(AuditLog auditLog)
    {
        try
        {
            await context.AuditLogs.AddAsync(auditLog);
            await context.SaveChangesAsync();

            logger.LogInformation("Audit log '{AuditId}' added successfully.", auditLog.Id.Value);
            return Result.Success();
        }
        catch (DbUpdateException exception)
        {
            logger.LogError(exception, "Failed to save audit log '{AuditId}'.", auditLog.Id.Value);
            return Result.Failure(AuditErrors.SaveFailed);
        }
    }
    
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<AuditLog>>> GetAsync(Id entityId)
    {
        try
        {
            var auditLogs = await context.AuditLogs
                .Where(al => al.EntityId == entityId)
                .ToListAsync();

            return Result.Success<IReadOnlyList<AuditLog>>(auditLogs);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to query expired audit logs.");
            return AuditErrors.QueryFailed;
        }
    }
}