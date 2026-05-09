namespace MadWorldEU.Byakko.Migrations;

/// <summary>Configuration options for automatic database migration on startup.</summary>
public sealed class MigrationOptions
{
    public const string SectionName = "Database:AutoMigrate";

    public bool Enabled { get; init; }
}