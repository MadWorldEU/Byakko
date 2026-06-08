# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
dotnet build
dotnet run --project src/MadWorldEU.Byakko.Controller.Api/Api.csproj
dotnet watch --project src/MadWorldEU.Byakko.Controller.Api/Api.csproj
dotnet test
dotnet test tests/MadWorldEU.Byakko.Controller.Api.IntegrationTests/ -- --coverage
```

API: `http://localhost:5062` / `https://localhost:7286`. OpenAPI at `/openapi/v1.json`, Scalar UI at `/scalar/v1`.

## Docker

Build all three from the **repository root**:

```bash
docker build -f src/MadWorldEU.Byakko.Controller.Api/Dockerfile .
docker build -f src/MadWorldEU.Byakko.Controller.Portal/Dockerfile .
docker build -f src/MadWorldEU.Byakko.Controller.Admin/Dockerfile .
```

Portal/Admin are nginx:alpine on port 8080 (non-root `nginx` user). Shared nginx config: `Controller.Blazor.Shared/DockerConfigs/nginx.conf`. CI builds multi-arch images to GHCR on `main`; production deploys on `v*` tags. See `docs/developer-guides/Pipelines.md`.

## Testing

See `.claude/rules/testing.md` for full conventions. Test projects:

| Project | Framework | Purpose |
|---|---|---|
| `Controller.Api.IntegrationTests` | Reqnroll + TUnit | BDD integration tests |
| `Core.Application.Unittests` | TUnit + NSubstitute + Shouldly | Use case error paths |
| `Core.Domain.Unittests` | TUnit + NSubstitute + Shouldly | Domain entity error paths |
| `Controller.Portal.Componenttests` | bUnit + WireMock.Net + TUnit | Portal Blazor components |
| `Controller.Admin.Componenttests` | bUnit + WireMock.Net + TUnit | Admin Blazor components |
| `ArchitectureTests` | ArchUnitNET + Reqnroll + TUnit | Layer dependency rules |

Key test notes:
- Integration tests use real PostgreSQL + MinIO Testcontainers — no mocks. `Authentication:ValidateUser = false` for self-signed tokens.
- Application unit tests cover error paths only (happy paths via integration tests). Use `Result.Failure<T>(error)` not `Result<T>.Failure(error)`.
- Domain unit tests use a `BuildAsset()` helper for constructing valid aggregates.
- Component tests: use `BunitContext` (not obsolete `TestContext`), `ctx.Render<T>()`, `cut.WaitForState()`, CSS selectors for assertions. Set `ctx.JSInterop.Mode = JSRuntimeMode.Loose` when the component calls JS interop (e.g. `initTooltips`). For async flows triggered by a button click, wait on a DOM change that only occurs after the async operation completes (e.g. wait for the confirm button to disappear rather than waiting on the spinner, which may not appear yet). Use `.TextContent.Trim()` when a `<td>` contains both text and child elements.
- Architecture tests: BDD feature files + `BaseArchitectureTests` (loads all assemblies via marker interfaces); step definitions scoped per feature with `[Scope(Feature = "...")]`; assertions via `rule.HasNoViolations(Architecture).ShouldBeTrue(rule.Description)`. Every assembly under test must have a marker interface (e.g. `IPostgresqlMarker`) in its root namespace.

## Architecture

Clean Architecture. Dependencies flow inward: Controller → Application → Domain ← Infrastructure.

**Enforced dependency rules** (verified by `ArchitectureTests`):
- **BuildingBlocks** — must not depend on any other layer.
- **Domain** — must not depend on Application, Contracts, Infrastructure, or Controllers.
- **Contracts** — must not depend on Application, Infrastructure, or Controllers.
- **Application** — must not depend on any Infrastructure layer (Postgresql, ObjectStorage, Security) or Controllers.
- **Infrastructure** (Postgresql, ObjectStorage, Security) — must not depend on Controllers.
- **Blazor projects** (Admin, Portal, Blazor.Shared) — must not depend on Domain, Application, any Infrastructure layer, or Controller.Api; may only depend on Contracts and Blazor.Shared.

| Layer | Project | Role |
|---|---|---|
| Controller | `Controller.Api` | ASP.NET Core Minimal APIs |
| Controller | `Controller.Admin` | Blazor WebAssembly admin UI |
| Controller | `Controller.Portal` | Blazor WebAssembly portal UI |
| Controller | `Controller.Blazor.Shared` | Razor Class Library shared by Admin and Portal |
| Application | `Core.Application` | Business logic / use cases |
| Contracts | `Core.Contracts` | Shared request/response DTOs |
| Domain | `Core.Domain` | Domain models / entities |
| BuildingBlocks | `Core.BuildingBlocks` | DDD base types |
| Infrastructure | `Infrastructure.Postgresql` | PostgreSQL (EF Core + Npgsql) |
| Infrastructure | `Infrastructure.ObjectStorage` | S3-compatible storage |
| Infrastructure | `Infrastructure.Security` | AES-256 encryption |
| Host | `Aspire` | .NET Aspire AppHost |

### DDD Building Blocks

`MadWorldEU.Byakko.Core.BuildingBlocks/DomainDrivenDevelopment/`, namespace `MadWorldEU.Byakko.DomainDrivenDevelopment`:

| Type | Usage |
|---|---|
| `AggregateRoot<TId>` | Root aggregate; exposes `RaiseDomainEvent` / `ClearDomainEvents` |
| `Entity<TId>` | Entity with identity; equality by `Id` |
| `ValueObject` | Immutable concept; structural equality via `GetEqualityComponents()` |
| `IDomainEvent` | Marker interface for domain events |
| `IGuidGenerator` / `GuidGenerator` | Abstracts `Guid.NewGuid()`; inject into use cases |

Register via `services.AddBuildingBlocks()`.

### Functional Patterns

`MadWorldEU.Byakko.Core.BuildingBlocks/Functional/`, namespace `MadWorldEU.Byakko.Functional`:

| Type | Usage |
|---|---|
| `Error` | `Error.Create("Domain.Reason", "description")` |
| `Result` | `Result.Success()` / `Result.Failure(error)` |
| `Result<T>` | Value implicitly converts to success; access via `.Value` |

```csharp
var response = result.Match(
    onSuccess: asset => new AssetResponse(asset.Id),
    onFailure: error => throw new NotFoundException(error.Description)
);
```

## Assets API

Endpoints in `Controller.Api/Endpoints/Storages/AssetsEndpoints.cs`:

| Method | Route | Use case | Notes |
|---|---|---|---|
| `GET` | `/assets` | `GetAssetsMetaDataUseCase` | Paged (20/page); requires `Administrator` policy; query param `page` |
| `POST` | `/assets` | `CreateAssetMetadataUseCase` | Creates record; validity from `Assets:ValidityPeriodInDays` |
| `GET` | `/assets/{id}` | `GetAssetMetadataUseCase` | 404 when not found; returns metadata even for deleted assets |
| `PUT` | `/assets/{id}/content` | `UploadAssetContentUseCase` | 404 not found, 403 not owner |
| `DELETE` | `/assets/{id}/content` | `DeleteContentOfAssetUseCase` | Requires `Administrator` policy; 404 not found, 409 already deleted |
| `GET` | `/assets/{id}/content` | `DownloadAssetContentUseCase` | 404 not found, 400 expired |

Content encrypted AES-256 before upload; IV prepended to ciphertext. Error mapping: check `error.Code` against `AssetErrors`, fall back to `400`.

**Asset lifecycle:**
- `ExpiresAt` — `CreatedAt + ValidityPeriodInDays`. `IsExpired(clock)` gates downloads.
- `DeletedAt` — set by `asset.Delete(clock)`. `IsDeleted` = `DeletedAt.HasValue`.
- `Delete(clock)` → `AssetErrors.AlreadyDeleted` if already deleted. Also sets `ExpiresAt = now` if it was still in the future.
- `UpdateSize(clock, size)` → `AssetErrors.SizeAlreadySet` if `Size.Value > 0`.
- `ValidityPeriod` value object enforces `Days > 0`.

**Manual cleanup triggers** (`ManualTriggersEndpoints.cs`, requires auth):
- `POST /host-services/manual-triggers/clean-up/assets-content` → `DeleteAllExpiredContentOfAssetsUseCase`
- `POST /host-services/manual-triggers/clean-up/assets-metadata` → `DeleteAllExpiredMetaDataAssetsUseCase`

## Storage Statistics API

Endpoint in `Controller.Api/Endpoints/Storages/GeneralStorageEndpoints.cs`:

| Method | Route | Use case | Notes |
|---|---|---|---|
| `GET` | `/storage/statistics` | `GetStorageStatisticsUseCase` | Requires `Administrator` policy; returns `GetStorageStatisticsResponse` |

`GetStorageStatisticsResponse` (in `Core.Contracts`) has `TotalFiles` (`int`) and `TotalBytes` (`long`) — counts and sums only non-deleted assets. `GetTotalSavedSizeAsync` uses `EF.Property<long>` + `Select` to work around EF Core's inability to translate `.Value` on a value-object property in `SumAsync`.

## Key Infrastructure

- **Database:** PostgreSQL via `ByakkoContext`. Connection string key: `byakko-db`. Auto-migrate: `Database:AutoMigrate` (default `false`; `true` in Development). Migrations in `Infrastructure.Postgresql/Migrations/`. See `docs/Database.md`.
- **Object storage:** `IAmazonS3` registered by `AddObjectStorage`. Connection string key: `localstack`. Settings: `Storage:Mode` (`LocalStack`/`OvhCloud`), `BucketName`, `AutoCreateBucket`. `DeleteAsync` uses `GetObjectMetadataAsync` + `DeleteObjectAsync` with `VersionId` (required for OVHCloud versioning).
- **Encryption:** AES-256-CBC via `AddSecurity`. Config key: `Encryption:Key` (base64 32-byte; `openssl rand -base64 32`). Random IV per call, prepended to ciphertext. `EncryptionService.Decrypt` returns `Result<T>`; catches `CryptographicException` / `EndOfStreamException` → `EncryptionErrors.DecryptionFailed`.
- **Aspire:** Provisions Postgres, LocalStack, Keycloak. `RunMode` in `appsettings.json`: `Project` / `DockerFile` / `ContainerImage`. See `docs/Aspire.md`.
- **CORS:** `Cors:AllowedOrigins` array; empty = allow any. Registered by `AddDefaultCors()`.
- **Security headers:** API via `SecurityHeadersMiddleware`. Portal/Admin via nginx config. Traefik via `middlewares.yaml`. `X-Frame-Options` intentionally omitted from Traefik (Keycloak needs frameability for OIDC).
- **Rate limiting:** `AddApiRateLimiter()`. Global 100 req/min; `content` policy 20 req/min for upload/download. Toggle: `RateLimiting:Enabled`. Disabled in Development and tests.
- **Auth:** JWT Bearer via Keycloak (`Authentication:Authority`). Blazor OIDC via `AddOidcAuthentication`; config in `wwwroot/appsettings.json` under `Oidc`. Bearer attached via `AuthorizationMessageHandler` on `HttpClients.ApiAuthorized`. Blazor OIDC settings bound via `IOptions<OidcSettings>` (shared); `OidcSettings.GetEditAccountUrl()` returns the Keycloak account page URL.
- **Asset settings:** `Assets:ValidityPeriodInDays` (default `30`), `Assets:MaxUploadSizeInBytes` (default `1073741824`). **Must be quoted string** in Helm values to prevent YAML integer coercion.
- **Scheduled cleanup:** Two `BackgroundService`s in `HostedServices/`, triggered daily at `Cleanup:TriggerHourUtc` (default `2`).
- **Observability:** OTLP via `OTEL_EXPORTER_OTLP_ENDPOINT`. When `observability.enabled`, exports to Tempo/Prometheus/Loki → Grafana.
- **OpenAPI server URL:** `OpenApi:ServerUrl` overridden by `ServerUrlDocumentTransformer` (Scalar "Try It Out").
- **Debug endpoints:** `/debug/*` — Development only. **Test endpoint:** `GET /tests/ping` — always registered.

## Admin UI

Blazor WebAssembly, Bootstrap 5 dark theme (`data-bs-theme="dark"`). Desktop-first with 240px sticky sidebar (`Layout/MainLayout.razor`). Nav sections: Overview, Management, Support, System. Auth via `<AuthorizeView Policy="@AuthorizationPolicies.Administrator">`. Sidebar footer shows username, edit-profile link (Keycloak account page via `OidcSettings.GetEditAccountUrl()`), and logout. Pages: `Pages/Home.razor` (`/dashboard` — two stat cards: Total Files and Storage Used, populated from `GET /storage/statistics` via `IStorageService`), `Pages/HostServices/ManualTriggers.razor`, `Pages/Storages/AssetsOverview.razor` (`/storages/assets` — paged asset table, 20/page, with previous/next pagination). The Management nav "Assets" link points to `/storages/assets`. `IAssetService` (shared) exposes `GetAssetsMetadataAsync(int page)` and `DeleteAssetContentAsync(Guid id)` for admin use; `IStorageService` (shared) exposes `GetStorageStatisticsAsync()` for the dashboard; `MadWorldEU.Byakko.Services`, `MadWorldEU.Byakko.Storages`, and `MadWorldEU.Byakko.Formatters` are global usings in `_Imports.razor`.

**AssetsOverview features:**
- Each row has an info icon (Bootstrap Icons info-circle SVG) next to the filename; hovering shows a Bootstrap tooltip (`data-bs-html="true"`) with file size and last updated date on separate lines. Tooltip initialization via `initTooltips()` in `wwwroot/js/app.js` (loaded in `index.html`), called from `OnAfterRenderAsync`.
- Each non-deleted asset row has a **Delete** button; clicking it shows an inline confirm prompt ("Delete content? Yes / No") in the same row. Confirming calls `DeleteAssetContentAsync` and reloads the page.
- `AssetMetadataResponse` (in `Core.Contracts`) includes `IsDeleted` and `Size` (bytes as `long`) in addition to the base fields.

## Portal UI

Blazor WebAssembly, Bootstrap 5 dark theme. Sticky top navbar with auth dropdown (username, edit-profile link via `OidcSettings.GetEditAccountUrl()`, logout). Pages: `Pages/Storage/Upload.razor` (`/storage/upload`), `Pages/Storage/Download.razor` (shows expiry warning client-side; server enforces). Max upload size from `IOptions<AssetSettings>` → `IBrowserFile.OpenReadStream(maxAllowedSize)`.

## Project Conventions

See `.claude/rules/code-standard.md` for full standards. Key points:

- **Framework:** .NET 10.0, `TreatWarningsAsErrors = true`
- **Namespace:** root `MadWorldEU.Byakko`; after root, mirror folder structure; never include project name
- **Access:** most restrictive possible; classes `sealed` by default
- **Central Package Management:** versions only in `Directory.Packages.props`
- **NodaTime:** always; `Instant` for UTC; inject `IClock`
- **`var`:** preferred everywhere
- **Global usings:** third-party group then `MadWorldEU.Byakko.*` group, each alphabetical
- **Endpoint classes:** `internal static` with `internal static` extension on `WebApplication`; under `Endpoints/<Feature>/`
- **HTTP clients:** `HttpClients.ApiAnonymous` / `HttpClients.ApiAuthorized` from `Controller.Blazor.Shared/HttpClients.cs`. `ApiAuthorized` has `Timeout = Timeout.InfiniteTimeSpan` — upload timeouts are governed by Traefik's `responseHeaderTimeout` (1800s), not the client.
- **Shared services:** `IAssetService`/`AssetService` and `IStorageService`/`StorageService` live in `Controller.Blazor.Shared/Services/` and are registered in `WebAssemblyHostBuilderExtensions.AddByakkoServices()`.
- **Shared formatters:** `ByteFormatter.Format(long?)` in `Controller.Blazor.Shared/Formatters/ByteFormatter.cs` — formats byte counts to B/KB/MB/GB using `InvariantCulture` (always `.` as decimal separator).
- **appsettings schemas:** each `appsettings.json` has a companion `appsettings-schema.json`; update schema when adding config keys
- **EF Core constructor:** `private`, `[UsedImplicitly]`, XML `<summary>` saying "Required for EF Core"
- **Errors:** `static readonly` fields on `{Domain}Errors` class; code format `"Domain.Reason"`

## Helm / Kubernetes

Chart in `deployments/helm/byakko/`. See `docs/developer-guides/Kubernetes.md` for setup.

Key templates: `api.yaml`, `portal.yaml`, `admin.yaml`, `database.yaml`, `keycloak.yaml`, `keycloak-database.yaml`, `pgadmin.yaml`, `localstack.yaml` (guarded by `localstack.enabled`), observability stack (`otel-collector`, `prometheus`, `tempo`, `loki`, `grafana` — guarded by `observability.enabled`), `ingress.yaml`, `middlewares.yaml`.

Subdomains: `byakko.dev`/`www.`/`fileshare.` → Portal; `api.` → API; `admin.` → Admin; `database.` → pgAdmin; `authentication.` → Keycloak; `grafana.` → Grafana.

TLS local: `mkcert byakko.dev "*.byakko.dev"` + `kubectl create secret tls`. TLS production: `clusterIssuer.enabled = true` + cert-manager HTTP-01. Remove AAAA records if no IPv6.

## Documentation & Diagrams

DocFX site at `https://madworldeu.github.io/Byakko`. Preview: `docfx docs/docfx.json --serve`.

C4 diagrams in `docs/diagrams/workspace.dsl` (Structurizr DSL). Edit locally:
```bash
docker run -it --rm -p 8080:8080 -v ./:/usr/local/structurizr structurizr/structurizr local
```