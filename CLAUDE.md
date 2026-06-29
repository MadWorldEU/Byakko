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

Build from the **repository root** with `-f src/MadWorldEU.Byakko.Controller.<Name>/Dockerfile .` (Api, Portal, Admin, Status). Portal/Admin are nginx:alpine on port 8080 (non-root `nginx` user). Status is aspnet:10.0 (Blazor Server). Shared nginx config: `Controller.Blazor.Shared/DockerConfigs/nginx.conf`. CI builds multi-arch images to GHCR on `main`; production deploys on `v*` tags.

**Portal Docker entrypoint:** `Controller.Portal/Docker/docker-entrypoint.sh` runs `envsubst '${OG_IMAGE_URL}'` on `index.html` at startup — only that variable is substituted, leaving all other `${}` content untouched.

## Testing

See `.claude/rules/testing.md` for full conventions. Test projects:

| Project | Framework | Purpose |
|---|---|---|
| `Controller.Api.IntegrationTests` | Reqnroll + TUnit | BDD integration tests for the API |
| `Controller.Status.IntegrationTests` | Reqnroll + TUnit + WireMock.Net | BDD integration tests for the Status dashboard |
| `Core.Application.Unittests` | TUnit + NSubstitute + Shouldly | Use case error paths |
| `Core.Domain.Unittests` | TUnit + NSubstitute + Shouldly | Domain entity error paths |
| `Controller.Blazor.Shared.Unittests` | TUnit + Shouldly | Shared Blazor use case logic |
| `Controller.Portal.Componenttests` | bUnit + WireMock.Net + TUnit | Portal Blazor components |
| `Controller.Admin.Componenttests` | bUnit + WireMock.Net + TUnit | Admin Blazor components |
| `ArchitectureTests` | ArchUnitNET + Reqnroll + TUnit | Layer dependency rules |

Key test notes:
- API integration tests use real PostgreSQL + LocalStack Testcontainers — no mocks. `Authentication:ValidateUser = false` for self-signed tokens.
- Status integration tests use real PostgreSQL + LocalStack + Mailpit Testcontainers; WireMock.Net stubs the 4 HTTP health endpoints returning `200 Healthy`. `MAILPIT_HOST`/`MAILPIT_PORT` injected via in-memory config.
- Application unit tests cover error paths only. Use `Result.Failure<T>(error)` not `Result<T>.Failure(error)`.
- Domain unit tests use a `BuildAsset()` helper for constructing valid aggregates.
- Component tests: use `BunitContext` (not obsolete `TestContext`), `ctx.Render<T>()`, `cut.WaitForState()`, CSS selectors. `ctx.JSInterop.Mode = JSRuntimeMode.Loose` for JS interop. `.TextContent.Trim()` for `<td>` with child elements. `IStringLocalizer<T>` requires `ctx.Services.AddLocalization()`.
- Architecture tests: BDD feature files + `BaseArchitectureTests`; every assembly needs a marker interface (e.g. `IPostgresqlMarker`) in its root namespace.

## Architecture

Clean Architecture. Dependencies flow inward: Controller → Application → Domain ← Infrastructure.

**Enforced dependency rules:**
- **BuildingBlocks** — no dependencies on other layers.
- **Domain** — must not depend on Application, Contracts, Infrastructure, or Controllers.
- **Contracts** — must not depend on Application, Infrastructure, or Controllers.
- **Application** — must not depend on any Infrastructure layer or Controllers.
- **Infrastructure** — must not depend on Controllers.
- **Blazor projects** (Admin, Portal, Blazor.Shared) — may only depend on Contracts and Blazor.Shared.

| Layer | Project | Role |
|---|---|---|
| Controller | `Controller.Api` | ASP.NET Core Minimal APIs |
| Controller | `Controller.Admin` | Blazor WebAssembly admin UI |
| Controller | `Controller.Portal` | Blazor WebAssembly portal UI |
| Controller | `Controller.Status` | Blazor Server status dashboard (public, no auth) |
| Controller | `Controller.Blazor.Shared` | Razor Class Library shared by Admin and Portal |
| Application | `Core.Application` | Business logic / use cases |
| Contracts | `Core.Contracts` | Shared request/response DTOs |
| Domain | `Core.Domain` | Domain models / entities |
| BuildingBlocks | `Core.BuildingBlocks` | DDD base types |
| Infrastructure | `Infrastructure.Postgresql` | PostgreSQL (EF Core + Npgsql) |
| Infrastructure | `Infrastructure.ObjectStorage` | S3-compatible storage |
| Infrastructure | `Infrastructure.Security` | AES-256 encryption |
| Infrastructure | `Infrastructure.Mail` | SMTP mail via MailKit |
| Host | `Aspire` | .NET Aspire AppHost |

### DDD Building Blocks

`Core.BuildingBlocks/DomainDrivenDevelopment/`, namespace `MadWorldEU.Byakko.DomainDrivenDevelopment`. Register via `services.AddBuildingBlocks()`. Call `IDomainEventsDispatcher.DispatchAsync(aggregate)` after a use case completes.

| Type | Usage |
|---|---|
| `AggregateRoot<TId>` | Root aggregate; `RaiseDomainEvent` / `ClearDomainEvents` |
| `Entity<TId>` | Identity equality by `Id` |
| `ValueObject` | Immutable; structural equality via `GetEqualityComponents()` |
| `IDomainEvent` | Marker interface |
| `IDomainEventHandler<T>` | One class per event type |
| `IDomainEventsDispatcher` | Resolves handlers from DI; cached `HandlerWrapper` pattern |
| `IGuidGenerator` | Abstracts `Guid.NewGuid()`; inject into use cases |

### Functional Patterns

`Core.BuildingBlocks/Functional/`, namespace `MadWorldEU.Byakko.Functional`. `Error.Create("Domain.Reason", "description")`. `Result.Success()` / `Result.Failure(error)`. `Result<T>` value implicitly converts to success. Use `result.Match(onSuccess, onFailure)`.

## Assets API

Endpoints in `Controller.Api/Endpoints/Storages/AssetsEndpoints.cs`:

| Method | Route | Use case | Notes |
|---|---|---|---|
| `GET` | `/assets` | `GetAssetsMetaDataUseCase` | Paged (20/page); `Administrator` policy; query param `page` |
| `POST` | `/assets` | `CreateAssetMetadataUseCase` | `ExpiresInDays` validated against `Assets:ValidityPeriodInDays` (max) |
| `GET` | `/assets/{id}` | `GetAssetMetadataUseCase` | 404 when not found; returns metadata even for deleted assets |
| `PUT` | `/assets/{id}/content` | `UploadAssetContentUseCase` | 404 not found, 403 not owner |
| `DELETE` | `/assets/{id}/content` | `DeleteContentOfAssetUseCase` | `Administrator` policy; 404 not found, 409 already deleted |
| `GET` | `/assets/{id}/content` | `DownloadAssetContentUseCase` | 404 not found, 400 expired; no password support |
| `POST` | `/assets/{id}/content` | `DownloadAssetContentUseCase` | Accepts `DownloadAssetContentRequest` with optional `Password`; `Encryption.DecryptionFailed` → 400 |

Content encrypted AES-256; salt (16 bytes) + IV prepended to ciphertext. Password: AES key derived via PBKDF2 (600 000 iterations, SHA-256) from `serverKey + password`. CORS exposes `Content-Disposition` via `WithExposedHeaders`.

**Asset lifecycle:**
- `ExpiresAt` = `CreatedAt + ValidityPeriodInDays`. `IsExpired(clock)` gates downloads.
- `Delete(clock)` → `AssetErrors.AlreadyDeleted` if already deleted; sets `ExpiresAt = now` if still in future.
- `UpdateSize(clock, size)` → `AssetErrors.SizeAlreadySet` if `Size.Value > 0`.
- `ValidityPeriod.Create(days, maxDays)` → `ValidityPeriodErrors.ExceedsMaximum` if `days >= maxDays`.
- `Size.Create(sizeInBytes, maxSizeInBytes)` → `AssetErrors.FileTooLarge` if exceeds max.

**Manual cleanup triggers** (`ManualTriggersEndpoints.cs`, requires auth):
- `POST /host-services/manual-triggers/clean-up/assets-content` → `DeleteAllExpiredContentOfAssetsUseCase`
- `POST /host-services/manual-triggers/clean-up/assets-metadata` → also bulk-deletes matching audit logs

## Correspondences API

`POST /correspondences/feedback` → `SendFeedbackUseCase`. Public; rate-limited `PublicPost` (1 req/60s per IP). `SendFeedbackRequest` carries `Email` + `Message`. `Email` value object: empty → `EmailErrors.Empty`; invalid format → `EmailErrors.Invalid`.

## Audits API

`GET /audits/{entityId}` → `GetAuditLogsUseCase`. `Administrator` policy. Returns `GetAuditLogsResponse` with `IReadOnlyList<AuditLogResponse>` (Id, EntityType, Action, IpAddress, OccurredAt, OccurredByUserId).

**Audit log domain** (`Core.Domain/Audits/`): `AuditLog` entity with `Id`, `EntityId`, `EntityType`, `Action`, `IpAddress`, `OccurredAt` (`Instant`), `OccurredBy`. `IpAddress.Create(null/empty)` → `AuditErrors.InvalidIpAddress`. Audit log deletion for expired assets is handled in `AssetRepository.DeleteExpiredAssets`, not via `IAuditRepository`.

**AuditAssetsHandler** handles `AssetMetaDataCreatedEvent` and `AssetContentUploadedEvent`; audit creation failure is non-fatal (logs warning, does not throw).

## Storage Statistics API

`GET /storage/statistics` → `GetStorageStatisticsUseCase`. `Administrator` policy. Returns `TotalFiles` (`int`) + `TotalBytes` (`long`) for non-deleted assets. Uses `EF.Property<long>` + `Select` to work around EF Core's inability to translate `.Value` on a value-object in `SumAsync`.

## Key Infrastructure

- **Database:** PostgreSQL via `ByakkoContext`. Connection string: `byakko-db`. Auto-migrate: `Database:AutoMigrate` (default `false`; `true` in Development). See `docs/Database.md`.
- **Object storage:** `IAmazonS3` via `AddObjectStorage`. Connection string: `localstack`. `Storage:Mode` (`LocalStack`/`OvhCloud`). `DeleteAsync` requires `VersionId` (OVHCloud versioning).
- **Encryption:** AES-256-CBC via `AddSecurity`. Config: `Encryption:Key` (base64 32-byte). `EncryptionService.Decrypt` catches `CryptographicException`/`EndOfStreamException` → `EncryptionErrors.DecryptionFailed`.
- **Aspire:** Provisions Postgres, LocalStack, Keycloak. `RunMode`: `Project`/`DockerFile`/`ContainerImage`. See `docs/Aspire.md`.
- **CORS:** `Cors:AllowedOrigins` array; empty = allow any. `AddDefaultCors()`.
- **Security headers:** API via `SecurityHeadersMiddleware`. Portal/Admin via nginx. Traefik via `middlewares.yaml`. `X-Frame-Options` omitted from Traefik (Keycloak needs frameability for OIDC).
- **Rate limiting:** `AddApiRateLimiter()`. Global 100 req/min; `content` 20 req/min; `public-post` 1 req/60s per IP. Toggle: `RateLimiting:Enabled`. Disabled in Development and tests.
- **Mail:** `AddMail(configuration)`. Config `Mail:` — `Mode` (`Smtp`/`MailPit`), `Host`, `Port`, `EnableSsl`, `Username`, `Token`, `AdministratorFrom`/`AdministratorTo`. `MailPit` mode overrides `Host`/`Port` from `MAILPIT_HOST`/`MAILPIT_PORT` env vars. `HasAuthentication` skips `Authenticate` when `Username` is empty. `MailFactory.Create` resolves lazily so test config overrides are always visible.
- **Auth:** JWT Bearer via Keycloak (`Authentication:Authority`). Blazor OIDC via `AddOidcAuthentication`; config under `Oidc` in `wwwroot/appsettings.json`. `OidcSettings.GetEditAccountUrl()` returns the Keycloak account page URL.
- **Keycloak theme:** Custom `byakko` theme (`parent=keycloak.v2`) overrides `termsText` in `en`/`nl`/`ja`. Aspire: bind-mounted from `Configurations/KeyCloak/themes/`; Helm: `keycloak-theme-byakko` ConfigMap. `TERMS_AND_CONDITIONS` required action with `defaultAction: true`.
- **Asset settings:** `Assets:ValidityPeriodInDays` (default `30`), `Assets:MaxUploadSizeInBytes` (default `1073741824`). **Must be quoted string** in Helm values.
- **Scheduled cleanup:** Two `BackgroundService`s in `HostedServices/`, daily at `Cleanup:TriggerHourUtc` (default `2`).
- **Observability:** OTLP via `OTEL_EXPORTER_OTLP_ENDPOINT` → Tempo/Prometheus/Loki → Grafana. Two collectors: `otel-collector` Deployment (OTLP from API/Status) + `otel-collector-logs` DaemonSet (pod stdout/stderr scraping).
- **IP forwarding:** `UseForwardedHeaders`. Integration tests set `X-Forwarded-For: 171.129.229.213` on all clients.
- **OpenAPI:** `OpenApi:ServerUrl` overridden by `ServerUrlDocumentTransformer`. Debug: `/debug/*` (Development only). Test: `GET /tests/ping`.

## Admin UI

Blazor WebAssembly, Bootstrap 5 dark theme. 240px sticky sidebar. Auth via `<AuthorizeView Policy="@AuthorizationPolicies.Administrator">`. Logo: `shield-shaded` SVG, fill `#0d6efd`.

| Page | Route | Notes |
|---|---|---|
| `Pages/Home.razor` | `/dashboard` | Stat cards (Total Files, Storage Used) via `IStorageService` |
| `Pages/Storages/AssetsOverview.razor` | `/storages/assets` | Paged (20/page); info tooltip (size + last updated); Logs button; Delete with inline confirm |
| `Pages/Audits/AuditLogs.razor` | `/audits/{EntityId:guid}` | All audit entries for one entity; back link to `/storages/assets` |
| `Pages/HostServices/ManualTriggers.razor` | — | Manual cleanup triggers |

Shared services: `IAssetService` (`GetAssetsMetadataAsync`, `DeleteAssetContentAsync`), `IStorageService` (`GetStorageStatisticsAsync`), `IAuditService` (`GetAuditLogsAsync`). Global usings in `_Imports.razor`: `MadWorldEU.Byakko.Services`, `.Storages`, `.Audits`, `.Audits.Summaries`, `.Formatters`.

## Portal UI

Blazor WebAssembly, Bootstrap 5 dark theme. Sticky top navbar. Logo: `cloud-download` SVG, fill `#0d6efd`. Supports English (`en`), Dutch (`nl-NL`), Japanese (`ja-JP`).

| Page | Route | Notes |
|---|---|---|
| `Pages/Home.razor` | `/` | Landing page with beta banner, hero, feature cards |
| `Pages/Storage/Upload.razor` | `/storage/upload` | File + advanced options (expiry picker, password with Generate button) |
| `Pages/Storage/Download.razor` | `/storage/download/{Id}` | Three states: expired/no content/ready; streaming progress bar; `IAsyncDisposable` |
| `Pages/Storage/MyAssets.razor` | `/storage/my-assets` | Paged own-assets table (status badges, size, expiry); `GET /assets/me` |
| `Pages/Contact.razor` | `/contact` | Public; email pre-filled from auth claim |
| `Pages/UserAgreement.razor` | `/user-agreement` | Static legal page |

**Upload:** date picker defaults to today + `MaxValidityPeriodInDays`. Generate calls `GeneratePasswordUseCase.Execute()`. Sends `POST /assets` then `PUT /assets/{id}/content` (multipart). Password sent as `null` when empty.

**Download:** JS `downloadFileWithPassword(url, password, dotNetRef)` streams response chunk-by-chunk, calls `UpdateProgress`, triggers blob download. `Encryption.DecryptionFailed` → wrong password message.

**Localization:** `BlazorWebAssemblyLoadAllGlobalizationData=true`. `Localization/Resources.resx` + `.nl-NL.resx` + `.ja-JP.resx`. `CultureResolver` reads localStorage, falls back to `navigator.language` (neutral-code matching), then `en`. `setCulture`/`getCulture` in `wwwroot/js/app.js`. Shared pages use `IStringLocalizer<SharedResources>` from `Blazor.Shared`. `AddLocalization()` in `AddByakkoServices()`.

**Components:** `UploadLimitsAlert.razor` — max file size + files remaining alert; used in `Upload.razor` and `MyAssets.razor`.

## Status UI

Blazor Server (static SSR), Bootstrap 5 dark theme. Public, no auth. Logo: `activity` SVG, fill `#0d6efd`. Single page (`/`) — Bootstrap card grid for 6 services via `GetHealthServicesUseCase` (parallel, 2s timeout). API/Portal/Admin/Authentication: HTTP GET (200=Healthy, `"Degraded"`=Degraded). Database: `CanConnectAsync()`. Object Storage: `ListBucketsAsync()`. Probe at `/health`.

## Project Conventions

See `.claude/rules/code-standard.md` for full standards. Key points:

- **Framework:** .NET 10.0, `TreatWarningsAsErrors = true`
- **Namespace:** root `MadWorldEU.Byakko`; mirror folder structure; never include project name
- **Access:** most restrictive possible; classes `sealed` by default
- **Central Package Management:** versions only in `Directory.Packages.props`
- **NodaTime:** always; `Instant` for UTC; inject `IClock`
- **`var`:** preferred everywhere
- **Global usings:** third-party group then `MadWorldEU.Byakko.*` group, each alphabetical
- **Endpoint classes:** `internal static` with `internal static` extension on `WebApplication`; under `Endpoints/<Feature>/`
- **HTTP clients:** `HttpClients.ApiAnonymous` / `HttpClients.ApiAuthorized`. `ApiAuthorized` has `Timeout = Timeout.InfiniteTimeSpan` — upload timeouts governed by Traefik's `responseHeaderTimeout` (1800s).
- **Shared services:** `IAssetService`, `IStorageService`, `IAuditService`, `ICorrespondenceService` in `Controller.Blazor.Shared/Services/`, registered in `AddByakkoServices()`. All return `ResultResponse<T>`. `SendFeedbackAsync(request, isAuthenticated)` handles `429` → `BlazorErrors.CorrespondenceTooManyRequests`.
- **Shared formatters:** `ByteFormatter.Format(long?)` — B/KB/MB/GB using `InvariantCulture`.
- **appsettings schemas:** companion `appsettings-schema.json`; update when adding config keys
- **EF Core constructor:** `private`, `[UsedImplicitly]`, XML `<summary>` "Required for EF Core"
- **Errors:** `static readonly` fields on `{Domain}Errors`; code format `"Domain.Reason"`

### Blazor HTTP Response Pattern

All Blazor service methods return `ResultResponse<T>` (`Controller.Blazor.Shared/Responses/`). `IsSuccess` / `Response` / `Failure` (`FailureResponse`: `Code`, `StatusCode`, `Description`). `EmptyResponse` for DELETE endpoints. `HttpClientExtensions`: `GetResultResponseFromJsonAsync`, `PostResultResponseFromJsonAsync`, `PutResultResponseFromJsonAsync` (raw `HttpContent`), `DeleteResultResponseFromJsonAsync`.

### Error Translation

`IErrorTranslator.Translate(FailureResponse)` / `Translate(code, defaultDescription)` — looks up `ErrorResources.resx` (+ `.nl-nl.resx`, `.ja-jp.resx`), falls back to `Description`. Add entries to all three `.resx` files when introducing a new error code.

**BlazorErrors** (`Controller.Blazor.Shared/Configurations/BlazorErrors.cs`): `FileLoadFailed`, `FilesLoadFailed`, `FileDeleteFailed`, `FileUploadFailed`, `FileInvalidId`, `AuditLogsLoadFailed`, `TriggerFailed`, `FeedbackSendFailed`, `CorrespondenceTooManyRequests`.

**Error handling in `catch` blocks:** log exception via `console.error`, show localized message via `ErrorTranslator`, use `_errorMessage` variable name.

## Helm / Kubernetes

Chart in `deployments/helm/byakko/`. See `docs/developer-guides/Kubernetes.md`.

Subdomains: `byakko.dev`/`www.`/`fileshare.` → Portal; `api.` → API; `admin.` → Admin; `status.` → Status; `database.` → pgAdmin; `authentication.` → Keycloak; `grafana.` → Grafana; `kubernetes.` → Headlamp (`headlamp.enabled`).

All image tags via top-level `appVersion` in `values.yaml`. Storage config (`Storage:Mode`, `BucketName`, `OvhCloud.*`) shared by `api.yaml` and `status.yaml`.

**Headlamp:** `ExternalName` Service → `my-headlamp.kube-system.svc.cluster.local`. Separate `byakko-ingress-headlamp` (no `security-headers` middleware — COOP/COEP break OIDC redirect). Requires `headlamp-client` in Keycloak + microk8s kube-apiserver OIDC args. See Kubernetes.md Step 6.

**Grafana Keycloak auth:** Generic OAuth in `grafana.yaml`. Role mapping: `contains(roles, 'Administrator') && 'Admin' || ''`. **Critical:** realm roles mapper must have **Add to ID token** enabled — Grafana falls back to ID token only. Client secret: `GRAFANA_KEYCLOAK_CLIENT_SECRET`.

**pgAdmin Keycloak auth:** `config_local.py` ConfigMap. `AUTHENTICATION_SOURCES = ['oauth2']`. Restricted via `administrator-only-browser` Keycloak flow. Client secret: `PGADMIN_KEYCLOAK_CLIENT_SECRET`.

**Grafana dashboards:** JSON files in `grafana/dashboards/` auto-loaded via `.Files.Glob`. Datasource names must match exactly (`"Loki"`, `"Prometheus"`, `"Tempo"`). Stream matchers: `.+` not `.*`.

**Observability retention (60 days):** Loki: `retention_period: 1440h` + `retention_enabled: true`. Prometheus: `--storage.tsdb.retention.time=60d`. Tempo: `block_retention: 1440h`.

TLS local: `mkcert byakko.dev "*.byakko.dev"`. TLS production: `clusterIssuer.enabled = true` + cert-manager HTTP-01.

## Documentation & Diagrams

DocFX site at `https://madworldeu.github.io/Byakko`. Preview: `docfx docs/docfx.json --serve`.

C4 diagrams in `docs/diagrams/workspace.dsl` (Structurizr DSL):
```bash
docker run -it --rm -p 8080:8080 -v ./:/usr/local/structurizr structurizr/structurizr local
```