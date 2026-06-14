namespace MadWorldEU.Byakko.Audits;

/// <summary>Domain errors for the audit log.</summary>
public static class AuditErrors
{
    public static readonly Error InvalidIpAddress = Error.Create("Audit.InvalidIpAddress", "The IP address could not be determined.");
}