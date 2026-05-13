# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
dotnet build                          # Build all projects
dotnet run --project src/MadWorldEU.Byakko.Controller.Api/Api.csproj
dotnet watch --project src/MadWorldEU.Byakko.Controller.Api/Api.csproj
```

The API runs on `http://localhost:5062` (HTTP) or `https://localhost:7286` (HTTPS).
OpenAPI spec is available at `/openapi/v1.json` in the Development environment.

## Testing

Three test projects exist:

| Project | Framework | Purpose |
|---|---|---|
| `MadWorldEU.Byakko.Controller.Api.IntegrationTests` | Reqnroll + TUnit | BDD integration tests for the API |
| `MadWorldEU.Byakko.Core.Application.Unittests` | TUnit + Shouldly | Unit tests for application logic |
| `MadWorldEU.Byakko.Controller.Admin.IntegrationTests` | — | Placeholder |
| `MadWorldEU.Byakko.Controller.Portal.IntegrationTests` | — | Placeholder |

```bash
dotnet test                                    # Run all tests
dotnet test tests/MadWorldEU.Byakko.Controller.Api.IntegrationTests/ -- --coverage  # With code coverage
```

**API integration tests** use `WebApplicationFactory<Program>` (in-process test server). The hook in `Hooks/ApiHooks.cs` spins up the factory once per test run and injects an `HttpClient` into each scenario via `ScenarioContext`. BDD scenarios live in `Features/` as `.feature` files; step definitions live in `StepDefinitions/`.

A **PostgreSQL Testcontainer** (`Testcontainers.PostgreSql`) and a **MinIO Testcontainer** (`Testcontainers.Minio`) are started in `BeforeTestRun` and torn down in `AfterTestRun`. The hook injects both connection strings via in-memory configuration and replaces the real `IAmazonS3` registration with one pointing at the container. `MigrateAsync` runs before any test executes. Tests always run against real, isolated infrastructure — no mocks, no shared dev services.

## Architecture

Clean Architecture with four layers:

| Layer | Project | Role |
|---|---|---|
| Controller | `MadWorldEU.Byakko.Controller.Api` | ASP.NET Core Minimal APIs |
| Controller | `MadWorldEU.Byakko.Controller.Admin` | Blazor WebAssembly admin UI |
| Controller | `MadWorldEU.Byakko.Controller.Portal` | Blazor WebAssembly portal UI |
| Application | `MadWorldEU.Byakko.Core.Application` | Business logic / use cases |
| Contracts | `MadWorldEU.Byakko.Core.Contracts` | Shared request/response DTOs |
| Domain | `MadWorldEU.Byakko.Core.Domain` | Domain models / entities |
| BuildingBlocks | `MadWorldEU.Byakko.Core.BuildingBlocks` | DDD base types (see below) |
| Infrastructure | `MadWorldEU.Byakko.Infrastructure.Postgresql` | PostgreSQL data access |
| Infrastructure | `MadWorldEU.Byakko.Instrastructure.ObjectStorage` | S3-compatible object storage (MinIO) |
| Host | `MadWorldEU.Byakko.Aspire` | .NET Aspire AppHost (orchestration) |

Dependencies flow inward: Controller → Application → Domain ← Infrastructure. Contracts are shared between Controllers and Application. BuildingBlocks is referenced by Domain and Infrastructure.

### DDD Building Blocks

Base types live in `MadWorldEU.Byakko.Core.BuildingBlocks/DomainDrivenDevelopment/`, namespace `MadWorldEU.Byakko.DomainDrivenDevelopment`:

| Type | Usage |
|---|---|
| `AggregateRoot<TId>` | Root of an aggregate; exposes `RaiseDomainEvent` / `ClearDomainEvents` |
| `Entity<TId>` | Any entity with an identity; equality is by `Id` |
| `ValueObject` | Immutable concepts (e.g. `Money`, `Address`); equality is structural via `GetEqualityComponents()` |
| `IDomainEvent` | Marker interface for domain events raised by aggregates |

Domain aggregates extend `AggregateRoot<TId>`, plain entities extend `Entity<TId>`, immutable value concepts extend `ValueObject`. EF Core entity classes live in the Infrastructure layer and are mapped to/from domain objects in repository implementations.

System utilities live in `MadWorldEU.Byakko.Core.BuildingBlocks/System/`, namespace `MadWorldEU.Byakko.System`:

| Type | Usage |
|---|---|
| `IGuidGenerator` / `GuidGenerator` | Abstracts `Guid.NewGuid()` for testability; inject into use cases and pass to domain factory methods |

Register via `services.AddBuildingBlocks()`. In tests, replace with a predictable implementation that returns a known `Guid`.

### Functional Patterns

Types live in `MadWorldEU.Byakko.Core.BuildingBlocks/Functional/`, namespace `MadWorldEU.Byakko.Functional`:

| Type | Usage |
|---|---|
| `Error` | Named error with a `Code` and `Description`; create via `Error.Create("Domain.Reason", "...")` |
| `Result` | Outcome of a void operation; use `Result.Success()` / `Result.Failure(error)` |
| `Result<T>` | Outcome of an operation that returns a value; access via `.Value` on success |

Both `Result` and `Result<T>` expose a `Match` method for functional-style handling:

```csharp
var response = result.Match(
    onSuccess: asset => new AssetResponse(asset.Id),
    onFailure: error => throw new NotFoundException(error.Description)
);
```

Application and domain methods should return `Result` or `Result<T>` instead of throwing exceptions for expected failure cases. A value can be implicitly converted to `Result<T>` (wraps as success), so methods can return the value directly:

```csharp
public Result<Asset> GetById(Guid id) => asset; // implicitly Result<Asset>.Success(asset)
```

## Key Infrastructure

- **Database:** PostgreSQL via `ByakkoContext` (EF Core + Npgsql). Connection string key: `byakko-db`. Configured in `appsettings.json`, overridden by Aspire at runtime. Migrations live in `Infrastructure.Postgresql/Migrations/`. See `docs/Database.md` for migration commands.
- **Object storage:** S3-compatible storage via `IAmazonS3` (AWSSDK). Registered by `AddObjectStorage()` in `Instrastructure.ObjectStorage`. Connection string key: `minio` (format: `Endpoint=...;AccessKey=...;SecretKey=...`). Storage mode and bucket name are configured under `Storage:Mode` and `Storage:BucketName` in `appsettings.json`. A `BucketInitializer` hosted service ensures the bucket exists on startup.
- **Migrations:** Automatic migration on startup is toggled via `Database:AutoMigrate` in `appsettings.json` (default `false`; `true` in `appsettings.Development.json`). When enabled, a `MigrationService` hosted service runs `MigrateAsync` before the app starts accepting requests. Manual commands are documented in `docs/Database.md`.
- **Aspire orchestration:** Postgres is provisioned with a data volume and pgAdmin. The API waits for the database; Admin and Portal wait for the API. Credentials are passed as Aspire parameters (`username`, `password`).
- **Health check:** `GET /health` on the API; `GET /health.txt` (static file) on Admin and Portal. Aspire monitors all three.
- **Observability:** OpenTelemetry tracing, metrics, and logging exported via OTLP. Endpoint configured via `OTEL_EXPORTER_OTLP_ENDPOINT` in `appsettings.json` (default `http://localhost:4040`); Aspire overrides this automatically with the dashboard endpoint.
- **Debug endpoints:** `GET /debug/info`, `GET /debug/environment/variables`, `GET /debug/memory`, `POST /debug/memory/gc` — only registered in the Development environment.

## Project Conventions

- **Target framework:** .NET 10.0
- **Nullable reference types** and **implicit usings** are enabled globally via `Directory.Build.props`
- **`TreatWarningsAsErrors = true`** — all warnings must be resolved
- **Central Package Management** — NuGet versions are defined only in `Directory.Packages.props`, not in individual `.csproj` files
- **Root namespace:** `MadWorldEU.Byakko`
- **Solution format:** `.slnx` (new Visual Studio format)
- **Indentation:** 4 spaces, LF line endings, UTF-8 (enforced by `.editorconfig`)
- **`var` usage:** preferred in all contexts (enforced by `.editorconfig`)
- **Global usings:** each project has a `GlobalUsings.cs`; third-party namespaces come first, then a blank line, then `MadWorldEU.Byakko.*` namespaces (both groups alphabetical)
- **NodaTime:** used for all date/time values. Use `Instant` for UTC timestamps (e.g. `CreatedAt`). Inject `IClock` into use cases instead of calling `SystemClock.Instance` directly
- **JetBrains annotations:** `JetBrains.Annotations` is used for IDE hints (e.g. `[UsedImplicitly]`)
- **TUnit:** test attributes (`[Test]`, etc.) are available without explicit usings via source generator