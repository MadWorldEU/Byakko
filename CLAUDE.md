# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
dotnet build                          # Build all projects
dotnet run --project src/MadWorldEU.Byakko.Controller.Api/Api.csproj
dotnet watch --project src/MadWorldEU.Byakko.Controller.Api/Api.csproj
```

The API runs on `http://localhost:5062` (HTTP) or `https://localhost:7286` (HTTPS).
OpenAPI spec is available at `/openapi/v1.json` in all environments.
Scalar API reference UI is available at `/scalar/v1` in all environments.

## Docker

Each controller project has a `Dockerfile`. All three must be built from the **repository root** using `-f` to point at the correct file:

```bash
docker build -f src/MadWorldEU.Byakko.Controller.Api/Dockerfile .
docker build -f src/MadWorldEU.Byakko.Controller.Portal/Dockerfile .
docker build -f src/MadWorldEU.Byakko.Controller.Admin/Dockerfile .
```

The API uses the `mcr.microsoft.com/dotnet/aspnet:10.0` runtime image and runs as the non-root `app` user. Portal and Admin are Blazor WebAssembly apps served by `nginx:alpine` on port 8080, running as the non-root `nginx` user; their shared nginx config lives in `Controller.Blazor.Shared/DockerConfigs/nginx.conf` (both Dockerfiles reference this single file). The nginx config sets `X-Frame-Options: DENY` and other security headers for local development. Both Dockerfiles delete the pre-compressed `appsettings.json.br` and `appsettings.json.gz` variants so that a Kubernetes `ConfigMap` volume mount can override `appsettings.json` at runtime without nginx serving a stale compressed copy.

The CI pipeline (`.github/workflows/docker-build-push.yml`) builds and pushes multi-arch images (`linux/amd64`, `linux/arm64`) to GHCR on every push to `main`. SonarCloud static analysis runs via `.github/workflows/sonarcloud.yml` on every push and pull request. The production deployment pipeline (`.github/workflows/deploy-production.yml`) has two jobs: a `dry-run` job that runs `helm template` on every pull request targeting `main` to validate chart rendering without cluster access, and a `deployment` job that runs `helm upgrade --install` against the production cluster on every `v*` tag push using the `vps-production` GitHub environment. Secrets are passed via `env:` blocks and referenced as shell variables. The `KUBECONFIG` secret must be stored as a base64-encoded string (`microk8s config | base64 -w 0`) — the workflow decodes it with `base64 -d` before writing to `~/.kube/config` to prevent YAML corruption. See `docs/developer-guides/Pipelines.md` for setup instructions for all pipelines.

## Testing

Six test projects exist:

| Project | Framework | Purpose |
|---|---|---|
| `MadWorldEU.Byakko.Controller.Api.IntegrationTests` | Reqnroll + TUnit | BDD integration tests for the API |
| `MadWorldEU.Byakko.Core.Application.Unittests` | TUnit + NSubstitute + Shouldly | Unit tests for application use cases (error paths) |
| `MadWorldEU.Byakko.Core.Domain.Unittests` | TUnit + NSubstitute + Shouldly | Unit tests for domain entity error paths |
| `MadWorldEU.Byakko.Controller.Portal.Componenttests` | bUnit + WireMock.Net + TUnit | Component tests for the Portal Blazor UI |
| `MadWorldEU.Byakko.Controller.Admin.Componenttests` | bUnit + WireMock.Net + TUnit | Component tests for the Admin Blazor UI |

```bash
dotnet test                                    # Run all tests
dotnet test tests/MadWorldEU.Byakko.Controller.Api.IntegrationTests/ -- --coverage  # With code coverage
```

**API integration tests** use `WebApplicationFactory<Program>` (in-process test server). The hook in `Hooks/ApiHooks.cs` spins up the factory once per test run and injects an `HttpClient` and `IServiceProvider` into each scenario via `ScenarioContext`. BDD scenarios live in `Features/` as `.feature` files; step definitions live in `StepDefinitions/`. `Authentication:ValidateUser = false` disables JWT signature validation so tests can use self-signed tokens from `Common/TestJwtToken.cs`. Steps that need direct database access retrieve `IServiceProvider` from `ScenarioContext` via `ScenarioContextKeys.ServiceProvider` and create a scope to resolve `ByakkoContext`.

A **PostgreSQL Testcontainer** (`Testcontainers.PostgreSql`) and a **LocalStack Testcontainer** (`Testcontainers.LocalStack`) are started in `BeforeTestRun` and torn down in `AfterTestRun`. The hook injects both connection strings via in-memory configuration and replaces the real `IAmazonS3` registration with one pointing at the container. `MigrateAsync` runs before any test executes. Tests always run against real, isolated infrastructure — no mocks, no shared dev services.

**Component tests** render Blazor components in-process using `BunitContext` (`using var ctx = new BunitContext()`). HTTP calls are intercepted by a `WireMockServer`. Use `ctx.Render<T>()` (not the obsolete `RenderComponent`), `cut.WaitForState(predicate, timeout)` for async init, and CSS selectors (`cut.Find`, `cut.FindAll`) for assertions.

**Application unit tests** cover error paths in use cases only — happy paths are covered by integration tests. Mocks are created with NSubstitute (`Substitute.For<T>()`). Async repository/storage methods return `Task.FromResult(...)`. Use `Result.Failure<T>(error)` (not `Result<T>.Failure(error)`) to produce a typed failure result.

**Domain unit tests** cover error paths on domain entities directly (no use cases, no mocks needed beyond `IClock` and `IGuidGenerator`). Use a `BuildAsset()` helper to construct a valid `Asset` via `Asset.Create(...)`, then exercise the method under test.

## Architecture

Clean Architecture with four layers:

| Layer | Project | Role |
|---|---|---|
| Controller | `MadWorldEU.Byakko.Controller.Api` | ASP.NET Core Minimal APIs |
| Controller | `MadWorldEU.Byakko.Controller.Admin` | Blazor WebAssembly admin UI |
| Controller | `MadWorldEU.Byakko.Controller.Portal` | Blazor WebAssembly portal UI |
| Controller | `MadWorldEU.Byakko.Controller.Blazor.Shared` | Razor Class Library shared by Admin and Portal |
| Application | `MadWorldEU.Byakko.Core.Application` | Business logic / use cases |
| Contracts | `MadWorldEU.Byakko.Core.Contracts` | Shared request/response DTOs |
| Domain | `MadWorldEU.Byakko.Core.Domain` | Domain models / entities |
| BuildingBlocks | `MadWorldEU.Byakko.Core.BuildingBlocks` | DDD base types (see below) |
| Infrastructure | `MadWorldEU.Byakko.Infrastructure.Postgresql` | PostgreSQL data access |
| Infrastructure | `MadWorldEU.Byakko.Infrastructure.ObjectStorage` | S3-compatible object storage (LocalStack / OvhCloud) |
| Infrastructure | `MadWorldEU.Byakko.Infrastructure.Security` | AES-256 encryption / decryption |
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
| `POST` | `/assets` | `CreateAssetMetadataUseCase` | Creates a new asset record (name + content type); validity period is read from `Assets:ValidityPeriodInDays` in appsettings |
| `GET` | `/assets/{id}` | `GetAssetMetadataUseCase` | Returns asset metadata; `404` when not found |
| `PUT` | `/assets/{id}/content` | `UploadAssetContentUseCase` | Uploads the binary content for an asset; `404` when not found, `403` when caller is not the owner |
| `GET` | `/assets/{id}/content` | `DownloadAssetContentUseCase` | Downloads the binary content of an asset; `404` when not found, `400` when expired (`AssetErrors.Expired`) |

The upload/download use cases delegate to `IContentStorage` and `IEncryptionService`; the metadata use cases delegate to `IAssetRepository`. Content is encrypted with AES-256 before upload and decrypted after download — the IV is randomly generated per call and prepended to the ciphertext in storage.

Endpoint error mapping follows a consistent pattern: check `error.Code` against known `AssetErrors` codes and return the appropriate HTTP status; unrecognised errors fall back to `400 Bad Request` with the error description.

### Asset lifecycle

`Asset` has two lifecycle timestamps beyond `CreatedAt` and `UpdatedAt`:

- `ExpiresAt` — computed at creation as `CreatedAt + ValidityPeriodInDays` days. When an asset's `ExpiresAt` is in the past and `DeletedAt` is null, the scheduled content cleanup considers it expired.
- `DeletedAt` — set by `asset.Delete(clock)` when the content cleanup use case soft-deletes the asset after removing its S3 content. Null means the asset is active. `IsDeleted` is a computed property (`DeletedAt.HasValue`).

`Asset` domain methods return `Result` and enforce invariants:

| Method | Validation |
|---|---|
| `Delete(clock)` | Returns `AssetErrors.AlreadyDeleted` if `IsDeleted` is already true |
| `UpdateSize(clock, size)` | Returns `AssetErrors.SizeAlreadySet` if `Size.Value > 0` (size may only be set once, on upload) |
| `IsExpired(clock)` | Returns `true` when `clock.GetCurrentInstant() > ExpiresAt`; used by `DownloadAssetContentUseCase` to gate downloads |

`ValidityPeriod` is a value object in `Core.Domain/Storages/` that enforces `Days > 0`.

### Cleanup endpoints (manual triggers)

Two manual-trigger endpoints live in `Controller.Api/Endpoints/HostServices/ManualTriggersEndpoints.cs`. They require authorization and call the same use cases as the scheduled services.

| Method | Route | Use case |
|---|---|---|
| `POST` | `/host-services/manual-triggers/clean-up/assets-content` | `DeleteAllExpiredContentOfAssetsUseCase` |
| `POST` | `/host-services/manual-triggers/clean-up/assets-metadata` | `DeleteAllExpiredMetaDataAssetsUseCase` |

## Key Infrastructure

- **Database:** PostgreSQL via `ByakkoContext` (EF Core + Npgsql). Connection string key: `byakko-db`. Configured in `appsettings.json`, overridden by Aspire at runtime. Migrations live in `Infrastructure.Postgresql/Migrations/`. See `docs/Database.md` for migration commands.
- **Object storage:** S3-compatible storage via `IAmazonS3` (AWSSDK). Registered by `AddObjectStorage(IConfiguration)` in `Infrastructure.ObjectStorage`. Connection string key: `localstack` (format: plain endpoint URL, e.g. `http://localhost:4566`). Storage settings are bound via `IOptions<StorageOptions>` from the `Storage` section in `appsettings.json`: `Mode` (`LocalStack` or `OvhCloud`), `BucketName`, `AutoCreateBucket` (default `false`; `true` in `appsettings.Development.json`), and a nested `OvhCloud` section (`Endpoint`, `AccessKey`, `SecretKey`, `Region`). `BucketInitializer` creates the bucket on startup only when `AutoCreateBucket` is `true`. The domain abstraction is `IContentStorage` (in `Core.Domain`); it uses `AssetPath` (bucket + key) to address objects and exposes `UploadAsync`, `DownloadAsync`, and `DeleteAsync`. `LocalStack` mode uses static `test`/`test` credentials and region `us-east-1`; `OvhCloud` mode reads all four credentials from `Storage:OvhCloud:*` and throws `InvalidOperationException` at startup if any value is missing. `DeleteAsync` uses `GetObjectMetadataAsync` to retrieve the current version ID and then calls `DeleteObjectAsync` with that `VersionId` to permanently remove the object — this is required when bucket versioning is enabled (OVHCloud), because calling `DeleteObjectAsync` without a `VersionId` only creates a delete marker. `ListObjectVersions` is not used because OVHCloud's S3 compatibility layer throws `NoSuchKey` for that call even when the object exists.
- **Encryption:** AES-256-CBC content encryption registered by `AddSecurity(IConfiguration)` in `Infrastructure.Security`. The domain abstraction is `IEncryptionService` (in `Core.Domain/Encryptions/`); it exposes `Encrypt(Stream)` / `Decrypt(Stream)` and `Encrypt(string)` / `Decrypt(string)` overloads. The implementation (`EncryptionService`) generates a random IV per call, prepends it to the ciphertext, and reads it back on decryption. Configuration key: `Encryption:Key` (base64-encoded 32-byte key; generate with `openssl rand -base64 32`). In production, supply the key via the `ENCRYPTION_KEY` GitHub secret which the deploy workflow passes as `--set api.encryption.key`.
- **Migrations:** Automatic migration on startup is toggled via `Database:AutoMigrate` in `appsettings.json` (default `false`; `true` in `appsettings.Development.json`). When enabled, a `MigrationService` hosted service runs `MigrateAsync` before the app starts accepting requests. Manual commands are documented in `docs/Database.md`.
- **Aspire orchestration:** Postgres (with pgAdmin), LocalStack (S3, persistent), and Keycloak are provisioned. The API waits for all three; Admin and Portal wait for the API. Credentials are passed as Aspire parameters (`db-username`, `db-password`, `keycloak-username`, `keycloak-password`). A companion `localstack-explorer` container provides a web UI for browsing LocalStack resources. The `Factories/` folder contains `IResourceFactory`, `ProjectResourceFactory`, `DockerFileResourceFactory`, `DockerContainerResourceFactory`, `ResourceFactoryBuilder`, and `ResourceBuilderExtensions` — this pattern abstracts how each service is run so `AppHost.cs` stays clean. Control which mode is used via `RunMode` in `appsettings.json`: `Project` (default, run from source), `DockerFile` (build from local Dockerfiles), or `ContainerImage` (pull pre-built images from GHCR). See `docs/Aspire.md` for details.
- **CORS:** Configured via `Cors:AllowedOrigins` in `appsettings.json`. When the array is non-empty, only those origins are allowed; an empty array falls back to allowing any origin. Registered by `AddDefaultCors()` in `Configurations/CorsExtensions.cs`. `UseCors()` is placed before `MapOpenApi()` and `MapScalarApiReference()` in `Program.cs` so CORS headers are present on OpenAPI and Scalar responses. The Helm chart sets five allowed origins: `api.`, root domain, `www.`, `fileshare.`, and `admin.`.
- **Security headers (API):** `SecurityHeadersMiddleware` in `Middlewares/SecurityHeadersMiddleware.cs` appends `X-Frame-Options: DENY` to every API response. Registered via `app.UseMiddleware<SecurityHeadersMiddleware>()` in `Program.cs` immediately after `UseForwardedHeaders`.
- **Security headers (Portal & Admin):** Set in the shared nginx config at `Controller.Blazor.Shared/DockerConfigs/nginx.conf`. Includes `X-Frame-Options: DENY`, `X-Content-Type-Options`, `Referrer-Policy`, `Permissions-Policy`, HSTS, CORP, and a Blazor-compatible CSP (`wasm-unsafe-eval`, `unsafe-inline` for styles). The `connect-src` and `frame-src` directives include localhost ports for local dev; in production these are overridden by the Helm `middlewares.yaml` Traefik middleware which uses the domain from `ingress.domain`.
- **Security headers (Traefik):** `middlewares.yaml` defines a `security-headers` Traefik `Middleware` applied to all ingress routes. Covers HSTS, CSP (with `connect-src` and `frame-src` using `ingress.domain`), `X-Content-Type-Options`, `Referrer-Policy`, `Permissions-Policy`, `Cross-Origin-Embedder-Policy: require-corp`, `Cross-Origin-Opener-Policy: same-origin`, and `Cross-Origin-Resource-Policy: same-site`. `X-Frame-Options` is intentionally omitted from the Traefik middleware — the API and Blazor apps handle it themselves, and Keycloak needs to be frameable by the portal for OIDC silent auth (configured in Keycloak's realm Security Defenses via `frame-ancestors`).
- **security.txt (RFC 9116):** Each app exposes `/.well-known/security.txt`. For Admin and Portal the file lives in `wwwroot/.well-known/security.txt` and is served by nginx. For the API the file lives in `wwwroot/.well-known/security.txt` and is served by `UseStaticFiles()` registered in `Program.cs`. In Kubernetes all three files are overridden via ConfigMap volume mounts so the contact address and expiry date can be changed without rebuilding images. The `securityTxt.contact`, `securityTxt.expires`, and `securityTxt.preferredLanguages` Helm values control the content; `contact` is passed as the `SECURITY_TXT_CONTACT` GitHub secret in production.
- **robots.txt:** Each app serves a `robots.txt` from `wwwroot/robots.txt` (nginx for Admin and Portal; `UseStaticFiles()` for the API). The Portal allows public crawling but blocks `/authentication/`, `/Development/`, and `/storage/`. The Admin and API both use `Disallow: /` to block all crawlers. These files are static and do not vary by environment, so there is no Helm override.
- **OpenAPI server URL:** The OpenAPI document's `servers` list is overridden by `ServerUrlDocumentTransformer` when `OpenApi:ServerUrl` is set in configuration. This ensures Scalar's "Try It Out" sends requests to the correct public host rather than the auto-detected localhost URL. Set to `https://localhost:7286` in `appsettings.json` for local development; the Helm chart sets it to `https://api.<domain>` via the `OpenApi__ServerUrl` environment variable.
- **Rate limiting:** Fixed-window rate limiting registered by `AddApiRateLimiter()` in `Configurations/RateLimiterExtensions.cs`. A global limiter (default 100 req/min) applies to all endpoints; upload and download additionally apply a stricter named `content` policy (default 20 req/min), both partitioned per authenticated user (Keycloak `sub` claim) with IP fallback. Toggle via `RateLimiting:Enabled` and tune limits via `RateLimiting:General` / `RateLimiting:Content` in `appsettings.json`. Disabled by default in `appsettings.Development.json` and in the API integration tests.
- **Authentication:** JWT Bearer authentication via Keycloak. Configured in `appsettings.json` under `Authentication:Authority`. The Keycloak realm defines `admin-client`, `api-client`, and `portal-client`. See `docs/AuthenticationServer.md` for setup.
- **OIDC (Blazor):** Both Admin and Portal use `Microsoft.AspNetCore.Components.WebAssembly.Authentication` for OIDC. Each app's `wwwroot/appsettings.json` contains an `Oidc` section (`Authority`, `ClientId`, `ResponseType`, `DefaultScopes`); `Program.cs` binds it via `AddOidcAuthentication`. Admin uses `admin-client`; Portal uses `portal-client`. `App.razor` wraps the router with `CascadingAuthenticationState` and uses `AuthorizeRouteView`. The `NotAuthorized` template in `AuthorizeRouteView` redirects authenticated users without the required role to `/access-denied` and unauthenticated users to `/authentication/login`. The OIDC redirect/callback is handled by `Pages/Authentication.razor` (`/authentication/{action}`). `AuthorizationMessageHandler` is registered on the `ApiAuthorized` named HTTP client so bearer tokens are attached automatically. Pages that must remain accessible without login need `@attribute [AllowAnonymous]`. Authorization policies (`Administrator`, `User`) and role constants are defined in `Controller.Blazor.Shared/Configurations/AuthorizationPolicies.cs` and `AuthorizationRoles.cs`; both apps register them via `AddByakkoAuthentication` in `WebAssemblyHostBuilderExtensions.cs`.
- **Health check:** `GET /health` on the API; `GET /health.txt` (static file) on Admin and Portal. Aspire monitors all three.
- **Observability:** OpenTelemetry tracing, metrics, and logging exported via OTLP. Endpoint configured via `OTEL_EXPORTER_OTLP_ENDPOINT` in `appsettings.json` (default `http://localhost:4040`); Aspire overrides this automatically with the dashboard endpoint. In Kubernetes, when `observability.enabled` is `true` the API automatically points to the in-cluster OTel Collector (port 4317 gRPC); the collector fans out traces to Tempo, metrics to Prometheus, and logs to Loki, all visualised in Grafana at `grafana.<domain>`.
- **Asset settings:** Two values are bound from the `Assets` section in `appsettings.json` via `AssetSettings` (`Core.Application/Storages/AssetSettings.cs`) and injected via `IOptions<AssetSettings>`:
  - `ValidityPeriodInDays` (default `30`) — injected into `CreateAssetMetadataUseCase`; validated as a `ValidityPeriod` value object (must be > 0) at request time.
  - `MaxUploadSizeInBytes` (default `1073741824` / 1 GB) — applied to the upload endpoint via `RequestSizeLimitAttribute` and `RequestFormLimitsAttribute` (both are required: Kestrel's body limit and the multipart parser each enforce their own limit). The Portal reads the same key from `wwwroot/appsettings.json` via `AssetSettings` in `Controller.Blazor.Shared/Configurations/AssetSettings.cs` (registered by `AddByakkoServices`) and passes it to `IBrowserFile.OpenReadStream(maxAllowedSize)`. Both Helm charts (`api.assets.*` and `portal.assets.*`) expose these values so they can be tuned per environment without rebuilding images.
- **Scheduled cleanup:** Two `BackgroundService` implementations in `Controller.Api/HostedServices/` run once per day at the UTC hour configured via `Cleanup:TriggerHourUtc` in `appsettings.json` (default `2`). `DeleteExpiredAssetsService` calls `DeleteAllExpiredContentOfAssetsUseCase` (removes S3 content and soft-deletes the asset record). `DeleteExpiredAssetMetaDataService` calls `DeleteAllExpiredMetaDataAssetsUseCase` (hard-deletes asset records where `DeletedAt` is not null **and** more than 365 days in the past — records soft-deleted within the last year are retained). Both services use `IServiceScopeFactory` to resolve scoped use cases. The trigger time is calculated by `CleanupSettings.CalculateDelayUntilNextTrigger(IClock)`.
- **Debug endpoints:** `GET /debug/info`, `GET /debug/environment/variables`, `GET /debug/memory`, `POST /debug/memory/gc` — only registered in the Development environment.
- **Test endpoints:** `GET /tests/ping` — always registered; returns `"pong"`, used to verify the API is reachable.

## Admin UI

The Admin (`MadWorldEU.Byakko.Controller.Admin`) is a Blazor WebAssembly app styled with Bootstrap 5, aimed at administrators managing files, customers, and support tickets.

- **Dark theme:** `data-bs-theme="dark"` is set on the `<html>` element in `wwwroot/index.html`, same mechanism as the Portal.
- **Bootstrap JS:** `lib/bootstrap/js/bootstrap.bundle.min.js` is loaded in `index.html` (before the Blazor script). The `libman.json` includes `js/**/*.*` to pull it in alongside the CSS.
- **Sidebar layout:** `Layout/MainLayout.razor` uses a persistent 240 px left sidebar on `md+` breakpoint (hidden on smaller screens — the admin is a desktop-first tool). The sidebar is `position: sticky; height: 100vh` so it stays visible while the main content scrolls. Nav links use Blazor's `<NavLink>` component for automatic `active` class highlighting. Nav items are grouped into four labelled sections: Overview, Management, Support, and System.
- **Auth in sidebar:** The sidebar nav and footer both use `<AuthorizeView Policy="@AuthorizationPolicies.Administrator">`. The nav section is only rendered for users with the `Administrator` role. The footer shows the account name with an icon-only logout button when authenticated (calls `Navigation.NavigateToLogout` to avoid the "not initiated from within the page" error), or a Login nav link when not authenticated.
- **Dashboard:** `Pages/Home.razor` is the admin landing page. It contains four stat cards (Total Files, Active Customers, Storage Used, Support Tickets) with coloured left-border accents, a Quick Actions column, a Recent Customers table, and a Support Tickets table — all showing empty states until real data is wired up.
- **Manual Triggers:** `Pages/HostServices/ManualTriggers.razor` (`/host-services/manual-triggers`, requires `[Authorize]`) shows two cards — one per cleanup job. Each card has a trigger button that disables and shows a spinner during the request, then displays a success or error alert. Uses `HttpClients.ApiAuthorized` so the bearer token is attached automatically.
- **Custom CSS:** `wwwroot/css/app.css` holds all admin-specific styles at the bottom of the file: sidebar layout, `.admin-nav-link` active/hover states, `.stat-card` hover lift and per-colour border variants (`stat-card-primary/success/info/warning`), `.stat-icon` sizing, and `.quick-action-btn` slide effect.

## Portal UI

The Portal (`MadWorldEU.Byakko.Controller.Portal`) is a Blazor WebAssembly app styled with Bootstrap 5.

- **Dark theme:** `data-bs-theme="dark"` is set on the `<html>` element in `wwwroot/index.html`. This activates Bootstrap 5.3's built-in dark colour scheme globally — no custom CSS variables needed for standard components.
- **Bootstrap JS:** `lib/bootstrap/js/bootstrap.bundle.min.js` is loaded in `index.html` (before the Blazor script) to enable interactive Bootstrap components such as the mobile navbar toggler.
- **SEO meta tags:** `wwwroot/index.html` includes `description`, `keywords`, and `author` meta tags, and Open Graph tags (`og:title`, `og:description`, `og:type`) for Facebook, LinkedIn, Discord, Slack, and other platforms. Twitter/X falls back to the Open Graph tags automatically.
- **Upload page:** `Pages/Storage/Upload.razor` (`/storage/upload`, requires `[Authorize]`) lets authenticated users pick a file and upload it. The maximum allowed size is read from `IOptions<AssetSettings>` (registered by `AddByakkoServices` from `Assets:MaxUploadSizeInBytes` in `wwwroot/appsettings.json`) and passed to `IBrowserFile.OpenReadStream(maxAllowedSize)`. `AssetSettings` is defined in `Controller.Blazor.Shared/Configurations/AssetSettings.cs`.
- **Layout:** `Layout/MainLayout.razor` wraps every page with a sticky top navbar (brand + collapsible nav) and a footer. Add new nav links there.
- **Auth in navbar:** The navbar uses `AuthorizeView`. When authenticated it shows a user-icon dropdown with the account name and a Logout item (calls `Navigation.NavigateToLogout` to avoid the "not initiated from within the page" error). When not authenticated it shows a Login button.
- **Home page:** `Pages/Home.razor` is the public-facing landing page. It contains a hero section, feature cards, a "how it works" step list, and a CTA band — all using Bootstrap utility classes plus custom styles in `wwwroot/css/app.css`.
- **Download page:** `Pages/Storage/Download.razor` shows file metadata (name, content type, expiry date) and a download link. When `ExpiresAt` is in the past, the page displays a warning alert and renders a disabled button instead of the download link. The expiry check uses `DateTimeOffset.UtcNow` on the client side as a UI hint; the authoritative check is enforced server-side by `DownloadAssetContentUseCase`.
- **Custom CSS:** `wwwroot/css/app.css` holds global styles. Portal-specific additions (hero gradient, feature card hover, step number circles) live at the bottom of that file under clearly labelled comment blocks.

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
- **Curly braces:** `if`, `else`, `for`, `foreach`, `while`, and `do` statements must always use curly braces, even for single-line bodies.
- **Endpoint classes:** use `internal static` with an `internal static` extension method on `WebApplication` (e.g. `AddAssetsEndpoints`). Grouped under `Endpoints/<Feature>/` within the API project.
- **HTTP clients (Admin & Portal):** Both Blazor WebAssembly projects use `IHttpClientFactory` with named clients defined in `HttpClients.cs` (in `Controller.Blazor.Shared`). `HttpClients.ApiAnonymous` is for unauthenticated requests; `HttpClients.ApiAuthorized` is for authenticated requests — it has `AuthorizationMessageHandler` registered so the bearer token is attached automatically to every call. The API base URL is configured via `ApiBaseUrl` in `wwwroot/appsettings.json` (default `https://localhost:7286`).
- **Blazor shared project:** `Controller.Blazor.Shared` is a Razor Class Library referenced by both Admin and Portal. It holds shared components and infrastructure (e.g. `HttpClients.cs`, `Pages/Authentication.razor`, `Pages/NotFound.razor`, `Pages/AccessDenied.razor`). Add code there when it is needed by both apps; keep app-specific layout, pages, and styles in each app's own project.
- **appsettings JSON schemas:** Each project that has an `appsettings.json` has a companion `appsettings-schema.json` in the same folder. The schema is referenced via `"$schema": "./appsettings-schema.json"` and provides IDE validation and tooltips for all configuration keys (including enums such as `RunMode` and `Storage:Mode`). When adding a new configuration key, update the schema to match.

## Helm / Kubernetes Deployment

The Helm chart lives in `deployments/helm/byakko/`. It deploys all services into a single Kubernetes namespace and is designed to work with MicroK8s + Traefik in production and Docker Desktop Kubernetes locally. See `docs/developer-guides/Kubernetes.md` for full setup instructions including MicroK8s installation, Traefik ingress, Headlamp dashboard, and DNS configuration.

### Chart structure

| Template | Kind(s) | Description |
|---|---|---|
| `namespace.yml` | Namespace | Creates the target namespace |
| `api.yaml` | Deployment, Service | ASP.NET Core API |
| `admin.yaml` | ConfigMap, Deployment, Service | Blazor WebAssembly admin UI (nginx); ConfigMap mounts `appsettings.json` with environment-specific `ApiBaseUrl` and OIDC settings |
| `portal.yaml` | ConfigMap, Deployment, Service | Blazor WebAssembly portal UI (nginx); same ConfigMap pattern as admin |
| `database.yaml` | Secret, StatefulSet, Service | PostgreSQL for the application |
| `keycloak.yaml` | Secret, StatefulSet (×2 Services) | Keycloak identity server |
| `keycloak-database.yaml` | Secret, StatefulSet, Service | Dedicated PostgreSQL for Keycloak |
| `pgadmin.yaml` | Secret, ConfigMap, Deployment, Service | pgAdmin 4 with both databases pre-registered |
| `localstack.yaml` | Deployment, Service | MinIO S3-compatible storage (development only, guarded by `localstack.enabled`) |
| `otel-collector.yaml` | ServiceAccount, ClusterRole, ClusterRoleBinding, ConfigMap (×2), Deployment, DaemonSet, Service | OpenTelemetry collector stack (guarded by `observability.enabled`); main Deployment receives OTLP and exports to Tempo/Loki/Prometheus; log DaemonSet tails pod logs on every node |
| `prometheus.yaml` | ConfigMap, StatefulSet, Service | Prometheus metrics store with remote-write receiver; guarded by `observability.enabled` |
| `tempo.yaml` | ConfigMap, StatefulSet, Service | Grafana Tempo trace store (OTLP HTTP on 4318, query API on 3200); guarded by `observability.enabled` |
| `loki.yaml` | ConfigMap, StatefulSet, Service | Grafana Loki log store with OTLP ingest; guarded by `observability.enabled` |
| `grafana.yaml` | Secret, ConfigMap (×3), Deployment, Service | Grafana pre-wired to Prometheus, Tempo, and Loki; guarded by `observability.enabled` |
| `ingress.yaml` | Ingress | Traefik ingress with TLS for all subdomains; adds `grafana.` when `observability.enabled`; references the `security-headers` middleware |
| `middlewares.yaml` | Middleware | Traefik `security-headers` middleware applied to all routes: HSTS, CSP, CORP, COEP, COOP, Permissions-Policy, X-Content-Type-Options, Referrer-Policy |
| `NOTES.txt` | — | Printed by Helm after install/upgrade |

### Values

`values.yaml` contains shared defaults. Override per environment in `values.production.yaml` or `values.development.yaml`. Key top-level sections:

| Key | Purpose |
|---|---|
| `namespace` | Kubernetes namespace for all resources |
| `ingress.domain` | Base domain (e.g. `byakko.dev`); subdomains are derived automatically |
| `ingress.tlsSecret` | Name of the TLS secret; created manually (mkcert) or by cert-manager |
| `clusterIssuer.enabled` | Set to `true` to use cert-manager for automatic TLS |
| `localstack.enabled` | Set to `true` in development to deploy MinIO in-cluster |
| `api.database.autoMigrate` | Set to `true` to run EF Core migrations on API startup |
| `api.storage.mode` | `LocalStack` (in-cluster MinIO) or `OvhCloud` |
| `api.storage.bucketName` | S3 bucket name; injected as `Storage__BucketName` env var |
| `api.storage.ovhCloud.endpoint` | OVHCloud S3 endpoint URL; injected as `Storage__OvhCloud__Endpoint` env var |
| `api.storage.ovhCloud.accessKey` | OVHCloud S3 access key; set in `Values.Production.yaml` |
| `api.storage.ovhCloud.secretKey` | OVHCloud S3 secret key; set in `Values.Production.yaml` |
| `api.storage.ovhCloud.region` | OVHCloud region (e.g. `de`); must match the subdomain in the endpoint URL |
| `api.encryption.key` | Base64-encoded 32-byte AES-256 key; passed via `ENCRYPTION_KEY` GitHub secret |
| `api.assets.validityPeriodInDays` | Number of days a newly created asset remains valid (default `30`); injected as `Assets__ValidityPeriodInDays` env var |
| `api.assets.maxUploadSizeInBytes` | Maximum upload size enforced by the API (default `"1073741824"` / 1 GB); injected as `Assets__MaxUploadSizeInBytes` env var. **Must be a quoted string** in `values.yaml` to prevent Helm YAML integer coercion from corrupting the value in the ConfigMap |
| `portal.assets.maxUploadSizeInBytes` | Maximum upload size enforced by the Portal UI before sending the request (default `"1073741824"` / 1 GB); injected into the Portal `appsettings.json` ConfigMap. **Must be a quoted string** for the same reason as `api.assets.maxUploadSizeInBytes` |
| `securityTxt.contact` | Contact URI for `security.txt` (e.g. `mailto:security@byakko.dev`); passed via `SECURITY_TXT_CONTACT` GitHub secret |
| `securityTxt.expires` | ISO 8601 expiry date for `security.txt` (e.g. `2027-12-31T23:59:59Z`) |
| `securityTxt.preferredLanguages` | Preferred languages for security reports (default `en`) |
| `keycloak.realm` | Keycloak realm name used to build the `Authentication:Authority` URL |
| `keycloak.audience` | Keycloak client ID used as the API audience |
| `admin.oidc.clientId` | Keycloak client ID injected into the Admin `appsettings.json` ConfigMap |
| `portal.oidc.clientId` | Keycloak client ID injected into the Portal `appsettings.json` ConfigMap |
| `observability.enabled` | Set to `true` to deploy the full observability stack (OTel Collector, Prometheus, Tempo, Loki, Grafana) |
| `observability.grafana.adminUser` | Grafana admin username |
| `observability.grafana.adminPassword` | Grafana admin password |

### Subdomains exposed via ingress

| Subdomain | Service |
|---|---|
| `byakko.dev` / `www.` / `fileshare.` | Portal |
| `api.` | API |
| `admin.` | Admin |
| `database.` | pgAdmin |
| `authentication.` | Keycloak |
| `grafana.` | Grafana (only when `observability.enabled`) |

### TLS

**Local development (mkcert):** create a locally-trusted certificate and store it as a Kubernetes secret:

```bash
mkcert -install
mkcert byakko.dev "*.byakko.dev"
kubectl create secret tls byakko-tls \
  --cert=byakko.dev+1.pem \
  --key=byakko.dev+1-key.pem \
  -n byakko
```

**Production (Let's Encrypt):** set `clusterIssuer.enabled = true` and `clusterIssuer.email` in `Values.Production.yaml`. cert-manager handles certificate issuance and renewal automatically via HTTP-01 challenge over port 80. DNS A records must be propagated and port 80 must be publicly reachable before the challenge can succeed. Remove any AAAA (IPv6) DNS records if the cluster does not have IPv6 configured, as Let's Encrypt will prefer IPv6 and fail with `Connection refused` if it is not bound.

## Documentation

The live documentation site is published at `https://madworldeu.github.io/Byakko` via GitHub Pages. It is built with DocFX from `docs/docfx.json` and deployed automatically on every push to `main`.

To build and preview locally:

```bash
dotnet tool update -g docfx   # install / update DocFX
docfx docs/docfx.json --serve # build and serve at http://localhost:8080
```

Article sources live in `docs/`. Developer guide articles live in `docs/developer-guides/`. API reference is generated from `src/**/*.csproj`. Generated output (`docs/_site/`) is excluded from version control.

## C4 Diagrams

Architecture diagrams follow the [C4 model](https://c4model.com/) and are defined in `docs/diagrams/workspace.dsl` using the Structurizr DSL. Exported SVGs live in `docs/diagrams/images/`.

To edit diagrams locally, run Structurizr Lite from the `docs/diagrams/` folder:

```bash
docker run -it --rm -p 8080:8080 -v ./:/usr/local/structurizr structurizr/structurizr local
```

Then open [http://localhost:8080](http://localhost:8080). You can edit the DSL directly (the browser reloads on save) or drag elements on the canvas. To export, use the export button in the top-right toolbar and save the SVG to `docs/diagrams/images/`. See `docs/developer-guides/Structurizr.md` for full details.