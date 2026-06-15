using MadWorldEU.Byakko.Audits.Summaries;

namespace MadWorldEU.Byakko.Audits;

/// <summary>Retrieves all audit log entries for a given entity.</summary>
public sealed class GetAuditLogsUseCase(IAuditRepository auditRepository)
{
    /// <summary>Returns all audit log entries for the specified entity ID.</summary>
    public async Task<Result<GetAuditLogsResponse>> QueryAsync(string entityId)
    {
        var entityIdResult = Id.Create(entityId);
        if (entityIdResult.IsFailure) return entityIdResult.Error;
        
        var auditLogsResult = await auditRepository.GetAsync(entityIdResult.Value);
        if (auditLogsResult.IsFailure) return auditLogsResult.Error;
        
        var logs = auditLogsResult.Value
            .Select(log => new AuditLogResponse
            {
                Id = log.Id.Value,
                Action = log.Action.ToString(),
                IpAddress = log.IpAddress.Value,
                OccurredAt = log.OccurredAt.ToDateTimeOffset(),
                OccurredByUserId = log.OccurredBy.Value
            })
            .ToList();

        return new GetAuditLogsResponse { Logs = logs };
    }
}