namespace MadWorldEU.Byakko.Audits;

/// <summary>The type of action recorded in an audit log entry.</summary>
public enum AuditAction
{
    Created = 0,
    Updated = 1,
    Uploaded = 2,
    SoftDeleted = 3,
    HardDeleted = 4
}