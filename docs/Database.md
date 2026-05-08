# Database

PostgreSQL is used as the database, accessed via EF Core with Npgsql. The `ByakkoContext` is the single DbContext for the application. Connection is configured via the `byakko-db` connection string, which Aspire overrides automatically at runtime.

## Migrations
This guide outlines how to manage database migrations using Entity Framework Core in the Umiko project.

### Pre-requisites
Ensure you have the dotnet-ef tool installed globally:
```bash
dotnet tool install --global dotnet-ef
```
If the tool is already installed, update it to the latest version:
```bash
dotnet tool update --global dotnet-ef
```

### Migrations
All commands should be run from the `src/MadWorldEU.Byakko.Controllers.Api` directory (the startup project).

#### Create Migration
```bash
dotnet ef migrations add <MigrationName> --context ByakkoContext --project ../MadWorldEU.Byakko.Infrastructure.Postgresql -o ../MadWorldEU.Byakko.Infrastructure.Postgresql/Migrations
```

#### Apply Migration
To apply the created migration to the database:
```bash
dotnet ef database update --context ByakkoContext --project ../MadWorldEU.Byakko.Infrastructure.Postgresql
```

#### Rollback
##### Listing All Migrations
To view all migrations:
```bash
dotnet ef migrations list --context ByakkoContext --project ../MadWorldEU.Byakko.Infrastructure.Postgresql
```

##### Rolling Back to a Specific Migration
To rollback to a specific migration (e.g., InitialCreate):
```bash
dotnet ef database update InitialCreate --context ByakkoContext --project ../MadWorldEU.Byakko.Infrastructure.Postgresql
```

##### Rolling Back All Migrations
To revert the database to its initial state (no migrations applied):
```bash
dotnet ef database update 0 --context ByakkoContext --project ../MadWorldEU.Byakko.Infrastructure.Postgresql
```

#### Removing the Last Migration
If you need to remove the last migration (without applying it to the database):
```bash
dotnet ef migrations remove --context ByakkoContext --project ../MadWorldEU.Byakko.Infrastructure.Postgresql
```