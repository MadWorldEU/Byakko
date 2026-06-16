using MadWorldEU.Byakko.Audits.Summaries;

namespace MadWorldEU.Byakko.Audits;

/// <summary>Response containing the audit log entries for a domain entity.</summary>
public sealed class GetAuditLogsResponse
{
    /// <summary>The list of audit log entries ordered by occurrence.</summary>
    public required IReadOnlyList<AuditLogResponse> Logs { get; init; }
}