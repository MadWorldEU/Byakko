namespace MadWorldEU.Byakko.Audits;

/// <summary>Domain errors for the audit log.</summary>
public static class AuditErrors
{
    public static readonly Error InvalidIpAddress = Error.Create("Audit.InvalidIpAddress", "The IP address could not be determined.");
    public static readonly Error SaveFailed = Error.Create("Audit.SaveFailed", "The audit log could not be saved.");
    public static readonly Error QueryFailed = Error.Create("Audit.QueryFailed", "The audit log could not be queried.");
}