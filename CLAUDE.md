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
Scalar API reference UI is available at `/scalar/v1` in the Development environment.

## Docker

Each controller project has a `Dockerfile`. All three must be built from the **repository root** using `-f` to point at the correct file:

```bash
docker build -f src/MadWorldEU.Byakko.Controller.Api/Dockerfile .
docker build -f src/MadWorldEU.Byakko.Controller.Portal/Dockerfile .
docker build -f src/MadWorldEU.Byakko.Controller.Admin/Dockerfile .
```

The API uses the `mcr.microsoft.com/dotnet/aspnet:10.0` runtime image. Portal and Admin are Blazor WebAssembly apps served by `nginx:alpine`; their nginx config lives in `DockerConfigs/nginx.conf` inside each project folder.

The CI pipeline (`.github/workflows/docker-build-push.yml`) builds and pushes multi-arch images (`linux/amd64`, `linux/arm64`) to GHCR on every push to `main`. SonarCloud static analysis runs via `.github/workflows/sonarcloud.yml` on every push and pull request. See `docs/Pipelines.md` for setup instructions for all pipelines.

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
| Infrastructure | `MadWorldEU.Byakko.Infrastructure.ObjectStorage` | S3-compatible object storage (MinIO) |
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

## Assets API

The Assets feature (`/assets`) is the primary domain feature. Endpoints live in `Controller.Api/Endpoints/Storages/AssetsEndpoints.cs`.

| Method | Route | Use case | Description |
|---|---|---|---|
| `POST` | `/assets` | `CreateAssetMetadataUseCase` | Creates a new asset record (name + content type) |
| `GET` | `/assets/{id}` | `GetAssetMetadataUseCase` | Returns asset metadata (id, name, content type, created at) |
| `PUT` | `/assets/{id}/content` | `UploadAssetContentUseCase` | Uploads the binary content for an asset |
| `GET` | `/assets/{id}/content` | `DownloadAssetContentUseCase` | Downloads the binary content of an asset |

The upload/download use cases delegate to `IContentStorage`; the metadata use cases delegate to `IAssetRepository`.

## Key Infrastructure

- **Database:** PostgreSQL via `ByakkoContext` (EF Core + Npgsql). Connection string key: `byakko-db`. Configured in `appsettings.json`, overridden by Aspire at runtime. Migrations live in `Infrastructure.Postgresql/Migrations/`. See `docs/Database.md` for migration commands.
- **Object storage:** S3-compatible storage via `IAmazonS3` (AWSSDK). Registered by `AddObjectStorage()` in `Infrastructure.ObjectStorage`. Connection string key: `minio` (format: `Endpoint=...;AccessKey=...;SecretKey=...`). Storage mode and bucket name are configured under `Storage:Mode` and `Storage:BucketName` in `appsettings.json`. A `BucketInitializer` hosted service ensures the bucket exists on startup. The domain abstraction is `IContentStorage` (in `Core.Domain`); it uses `AssetPath` (bucket + key) to address objects and exposes `UploadAsync` and `DownloadAsync`.
- **Migrations:** Automatic migration on startup is toggled via `Database:AutoMigrate` in `appsettings.json` (default `false`; `true` in `appsettings.Development.json`). When enabled, a `MigrationService` hosted service runs `MigrateAsync` before the app starts accepting requests. Manual commands are documented in `docs/Database.md`.
- **Aspire orchestration:** Postgres (with pgAdmin), MinIO, and Keycloak are provisioned with data volumes. The API waits for all three; Admin and Portal wait for the API. Credentials are passed as Aspire parameters (`db-username`, `db-password`, `minio-username`, `minio-password`, `keycloak-username`, `keycloak-password`). Set `RunMode:UseDockerFile` in `appsettings.json` to `true` to run services from their Dockerfiles instead of project references (useful for testing the production image locally). The `Factories/` folder in the Aspire project contains `IResourceFactory`, `ProjectResourceFactory`, `DockerResourceFactory`, `ResourceFactoryBuilder`, and `ResourceBuilderExtensions` — this pattern abstracts the difference between project and Dockerfile resources so `AppHost.cs` stays clean.
- **CORS:** The API registers a default CORS policy (`AddCors` / `UseCors`) that allows any origin, method, and header. Intended for development; should be tightened to explicit origins before production.
- **Authentication:** JWT Bearer authentication via Keycloak. Configured in `appsettings.json` under `Authentication:Authority`. The Keycloak realm defines `admin-client`, `api-client`, and `portal-client`. See `docs/AuthenticationServer.md` for setup.
- **Health check:** `GET /health` on the API; `GET /health.txt` (static file) on Admin and Portal. Aspire monitors all three.
- **Observability:** OpenTelemetry tracing, metrics, and logging exported via OTLP. Endpoint configured via `OTEL_EXPORTER_OTLP_ENDPOINT` in `appsettings.json` (default `http://localhost:4040`); Aspire overrides this automatically with the dashboard endpoint.
- **Debug endpoints:** `GET /debug/info`, `GET /debug/environment/variables`, `GET /debug/memory`, `POST /debug/memory/gc` — only registered in the Development environment.
- **Test endpoints:** `GET /tests/ping` — always registered; returns `"pong"`, used to verify the API is reachable.

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
- **Endpoint classes:** use `internal static` with an `internal static` extension method on `WebApplication` (e.g. `AddAssetsEndpoints`). Grouped under `Endpoints/<Feature>/` within the API project.
- **HTTP clients (Admin & Portal):** Both Blazor WebAssembly projects use `IHttpClientFactory` with named clients defined in `HttpClients.cs`. `HttpClients.ApiAnonymous` is for unauthenticated requests; `HttpClients.ApiAuthorized` is for authenticated requests. The API base URL is configured via `ApiBaseUrl` in `wwwroot/appsettings.json` (default `https://localhost:7286`).

## Documentation

The live documentation site is published at `https://madworldeu.github.io/Byakko` via GitHub Pages. It is built with DocFX from `docs/docfx.json` and deployed automatically on every push to `main`.

To build and preview locally:

```bash
dotnet tool update -g docfx   # install / update DocFX
docfx docs/docfx.json --serve # build and serve at http://localhost:8080
```

Article sources live in `docs/`. API reference is generated from `src/**/*.csproj`. Generated output (`docs/_site/`) is excluded from version control.

## C4 Diagrams

Architecture diagrams follow the [C4 model](https://c4model.com/) and are defined in `docs/diagrams/workspace.dsl` using the Structurizr DSL. Exported SVGs live in `docs/diagrams/images/`.

To edit diagrams locally, run Structurizr Lite from the `docs/diagrams/` folder:

```bash
docker run -it --rm -p 8080:8080 -v ./:/usr/local/structurizr structurizr/structurizr local
```

Then open [http://localhost:8080](http://localhost:8080). You can edit the DSL directly (the browser reloads on save) or drag elements on the canvas. To export, use the export button in the top-right toolbar and save the SVG to `docs/diagrams/images/`. See `docs/Structurizr.md` for full details.