namespace MadWorldEU.Byakko.Migrations;

/// <summary>Configuration options for automatic database migration on startup.</summary>
internal sealed class MigrationOptions
{
    internal const string SectionName = "Database";

    internal bool AutoMigrate { get; init; }
}