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
docker build -f src/MadWorldEU.Byakko.Controller.Status/Dockerfile .
```

Portal/Admin are nginx:alpine on port 8080 (non-root `nginx` user). Status is aspnet:10.0 (Blazor Server). Shared nginx config: `Controller.Blazor.Shared/DockerConfigs/nginx.conf`. CI builds multi-arch images to GHCR on `main`; production deploys on `v*` tags. See `docs/developer-guides/Pipelines.md`.

## Testing

See `.claude/rules/testing.md` for full conventions. Test projects:

| Project | Framework | Purpose |
|---|---|---|
| `Controller.Api.IntegrationTests` | Reqnroll + TUnit | BDD integration tests for the API |
| `Controller.Status.IntegrationTests` | Reqnroll + TUnit + WireMock.Net | BDD integration tests for the Status dashboard |
| `Core.Application.Unittests` | TUnit + NSubstitute + Shouldly | Use case error paths |
| `Core.Domain.Unittests` | TUnit + NSubstitute + Shouldly | Domain entity error paths |
| `Controller.Blazor.Shared.Unittests` | TUnit + Shouldly | Shared Blazor use case logic (e.g. `GeneratePasswordUseCase`) |
| `Controller.Portal.Componenttests` | bUnit + WireMock.Net + TUnit | Portal Blazor components |
| `Controller.Admin.Componenttests` | bUnit + WireMock.Net + TUnit | Admin Blazor components |
| `ArchitectureTests` | ArchUnitNET + Reqnroll + TUnit | Layer dependency rules |

Key test notes:
- API integration tests use real PostgreSQL + LocalStack Testcontainers — no mocks. `Authentication:ValidateUser = false` for self-signed tokens.
- Status integration tests use real PostgreSQL + LocalStack Testcontainers for database/storage checks; WireMock.Net stubs the 4 HTTP health endpoints (API, Portal, Admin, Authentication) returning `200 Healthy`. No auth (Status is public). The `the service X should have status Y` step uses a regex proximity check on the prerendered HTML.
- Application unit tests cover error paths only (happy paths via integration tests). Use `Result.Failure<T>(error)` not `Result<T>.Failure(error)`.
- Domain unit tests use a `BuildAsset()` helper for constructing valid aggregates.
- Component tests: use `BunitContext` (not obsolete `TestContext`), `ctx.Render<T>()`, `cut.WaitForState()`, CSS selectors for assertions. Set `ctx.JSInterop.Mode = JSRuntimeMode.Loose` when the component calls JS interop (e.g. `initTooltips`). For async flows triggered by a button click, wait on a DOM change that only occurs after the async operation completes (e.g. wait for the confirm button to disappear rather than waiting on the spinner, which may not appear yet). Use `.TextContent.Trim()` when a `<td>` contains both text and child elements. When multiple `<td>` elements share a class, use `:not(.other-class)` to target the right one (e.g. `td.small:not(.font-monospace)` to reach the date cell when IP and user ID cells also carry `small`). Pass component parameters via `ctx.Render<T>(p => p.Add(x => x.Prop, value))`. Any component that injects `IStringLocalizer<T>` requires `ctx.Services.AddLocalization()` in the test setup — omitting it causes a `InvalidOperationException: Cannot provide a value for property 'Localizer'` at render time.
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
| Controller | `Controller.Status` | Blazor Server status dashboard (public, no auth) |
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
| `IDomainEventHandler<T>` | Handle a specific domain event type; implement per handler class |
| `IDomainEventsDispatcher` / `DomainEventsDispatcher` | Dispatches domain events raised on aggregates to all registered handlers; resolves `IDomainEventHandler<T>` from DI via `IServiceProvider.GetServices()` using a cached `HandlerWrapper` pattern |
| `IGuidGenerator` / `GuidGenerator` | Abstracts `Guid.NewGuid()`; inject into use cases |

Register via `services.AddBuildingBlocks()`. After a use case completes its work, call `IDomainEventsDispatcher.DispatchAsync(aggregate)` to fire all queued domain events.

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
| `GET` | `/assets/{id}/content` | `DownloadAssetContentUseCase` | 404 not found, 400 expired; no password support |
| `POST` | `/assets/{id}/content` | `DownloadAssetContentUseCase` | Same errors; accepts `DownloadAssetContentRequest` JSON body with optional `Password`; `Encryption.DecryptionFailed` → 400 on wrong password |

Content encrypted AES-256 before upload; salt (16 bytes) + IV prepended to ciphertext. When a password is supplied, the AES key is derived via PBKDF2 (600 000 iterations, SHA-256) from `serverKey + password` with a per-file random salt stored alongside the ciphertext. Files without a password use the server key directly. Error mapping: check `error.Code` against `AssetErrors` and `EncryptionErrors`, fall back to `400`. CORS exposes `Content-Disposition` via `WithExposedHeaders` so the JS download function can read the filename.

**Asset lifecycle:**
- `ExpiresAt` — `CreatedAt + ValidityPeriodInDays`. `IsExpired(clock)` gates downloads.
- `DeletedAt` — set by `asset.Delete(clock)`. `IsDeleted` = `DeletedAt.HasValue`.
- `Delete(clock)` → `AssetErrors.AlreadyDeleted` if already deleted. Also sets `ExpiresAt = now` if it was still in the future.
- `UpdateSize(clock, size)` → `AssetErrors.SizeAlreadySet` if `Size.Value > 0`.
- `ValidityPeriod` value object enforces `Days > 0`.

**Manual cleanup triggers** (`ManualTriggersEndpoints.cs`, requires auth):
- `POST /host-services/manual-triggers/clean-up/assets-content` → `DeleteAllExpiredContentOfAssetsUseCase`
- `POST /host-services/manual-triggers/clean-up/assets-metadata` → `DeleteAllExpiredMetaDataAssetsUseCase` — also bulk-deletes all audit logs whose `EntityId` matches the deleted assets (fetches IDs first, then `ExecuteDeleteAsync` on `AuditLogs`, then on `Assets`)

## Audits API

Endpoint in `Controller.Api/Endpoints/Audits/AuditEndpoints.cs`:

| Method | Route | Use case | Notes |
|---|---|---|---|
| `GET` | `/audits/{entityId}` | `GetAuditLogsUseCase` | Requires `Administrator` policy; returns `GetAuditLogsResponse` |

`GetAuditLogsResponse` (in `Core.Contracts/Audits/`) has `IReadOnlyList<AuditLogResponse> Logs`. Each `AuditLogResponse` (in `Core.Contracts/Audits/Summaries/`) carries: `Guid Id`, `string EntityType`, `string Action`, `string IpAddress`, `DateTimeOffset OccurredAt`, `Guid OccurredByUserId`.

**Audit log domain** (`Core.Domain/Audits/`):
- `AuditLog` entity — `Id`, `EntityId`, `EntityType` (`AuditEntityType` enum), `Action` (`AuditAction` enum), `IpAddress` value object, `OccurredAt` (`Instant`), `OccurredBy` (`Id`). Created via `AuditLog.Create(...)`.
- `IpAddress` value object — wraps `string Value`; two `Create` overloads: `Create(string)` and `Create(System.Net.IPAddress?)`.  Returns `AuditErrors.InvalidIpAddress` when null/empty.
- `AuditAction` enum — actions recorded on entities (e.g. `Created`, `Uploaded`).
- `AuditEntityType` enum — types of domain entities that can be audited.
- `AuditErrors` — `InvalidIpAddress`, `SaveFailed`, `QueryFailed`.
- `IAuditRepository` — `AddAsync(AuditLog)` and `GetAsync(Id entityId)`; PostgreSQL implementation in `Infrastructure.Postgresql/Audits/AuditRepository.cs`. Audit log deletion for expired assets is handled directly in `AssetRepository.DeleteExpiredAssets`, not through `IAuditRepository`.

**AuditAssetsHandler** (`Core.Application/Audits/AuditAssetsHandler.cs`) — `IDomainEventHandler<AssetMetaDataCreatedEvent>` and `IDomainEventHandler<AssetContentUploadedEvent>`; writes an `AuditLog` entry per event. Audit creation failure is non-fatal: logs a warning via `ILogger` and returns without throwing.

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
- **Auth:** JWT Bearer via Keycloak (`Authentication:Authority`). Blazor OIDC via `AddOidcAuthentication`; config in `wwwroot/appsettings.json` under `Oidc`. Bearer attached via `AuthorizationMessageHandler` on `HttpClients.ApiAuthorized`. Blazor OIDC settings bound via `IOptions<OidcSettings>` (shared); `OidcSettings.GetEditAccountUrl()` returns the Keycloak account page URL. `Controller.Blazor.Shared/Pages/AccessDenied.razor` (`/access-denied`) — shown when a user lacks the required role; explains that access requires manual admin approval.
- **Keycloak theme:** A custom `byakko` login theme (`parent=keycloak.v2`) overrides the `termsText` message key in three languages: `messages_en.properties`, `messages_nl.properties`, and `messages_ja.properties`. `theme.properties` sets `internationalizationEnabled=true`, `supportedLocales=en,nl,ja`, `defaultLocale=en`. In Aspire the theme folder is bind-mounted from `Configurations/KeyCloak/themes/`; in Helm it is injected via the `keycloak-theme-byakko` ConfigMap (with volume mounts for all three `.properties` files). The realm JSON (`MadWorld-realm.json`) has `"internationalizationEnabled": true` so Keycloak respects the `ui_locales` OIDC parameter sent by the Portal. The Portal sets `ui_locales` via `builder.Services.PostConfigure<RemoteAuthenticationOptions<OidcProviderOptions>>` using `CultureInfo.CurrentUICulture.TwoLetterISOLanguageName`. The `TERMS_AND_CONDITIONS` required action is enabled with `defaultAction: true` so every user must accept on first login. See `docs/developer-guides/AuthenticationServer.md` for setup steps.
- **Asset settings:** `Assets:ValidityPeriodInDays` (default `30`), `Assets:MaxUploadSizeInBytes` (default `1073741824`). **Must be quoted string** in Helm values to prevent YAML integer coercion.
- **Scheduled cleanup:** Two `BackgroundService`s in `HostedServices/`, triggered daily at `Cleanup:TriggerHourUtc` (default `2`).
- **Observability:** OTLP via `OTEL_EXPORTER_OTLP_ENDPOINT`. When `observability.enabled`, exports to Tempo/Prometheus/Loki → Grafana. Both API and Status projects export OTLP. Logging uses `IncludeFormattedMessage = true` and `IncludeScopes = true`. Both projects set two OTel resource attributes at startup: `log_source: "application"` and `service_name: "Api"` / `"Status"` — these become Loki stream labels, allowing the Grafana dashboard to filter OTLP app logs by `log_source="application"`. Two OTel collectors run in Kubernetes: (1) **`otel-collector` Deployment** — receives OTLP pushes from API/Status on ports 4317 (gRPC) / 4318 (HTTP), enriches with `k8sattributes`, forwards logs → Loki, traces → Tempo, metrics → Prometheus; (2) **`otel-collector-logs` DaemonSet** — scrapes raw container stdout/stderr from `/var/log/pods/` on each node, parses CRI/Docker format, extracts pod metadata from file path, enriches with `k8sattributes`, stamps `log_source: k8s_pods`, forwards → Loki. In Loki: `log_source="application"` = API/Status OTLP logs; `log_source="k8s_pods"` = all other pod stdout/stderr.
- **IP address forwarding:** API reads `X-Forwarded-For` via `UseForwardedHeaders`. Integration tests set `X-Forwarded-For: 171.129.229.213` as a default header on both anonymous and authenticated `HttpClient`s so `HttpContext.Connection.RemoteIpAddress` is populated for audit logging.
- **OpenAPI server URL:** `OpenApi:ServerUrl` overridden by `ServerUrlDocumentTransformer` (Scalar "Try It Out").
- **Debug endpoints:** `/debug/*` — Development only. **Test endpoint:** `GET /tests/ping` — always registered.

## Admin UI

**Logo/favicon:** `wwwroot/icons/logo.svg` — Bootstrap Icons `shield-shaded`, fill `#0d6efd`. Used in sidebar header (22×22) and mobile header (18×18) via `<img>`. Favicon wired in `wwwroot/index.html` as `<link rel="icon" type="image/svg+xml" href="/icons/logo.svg" />`.

Blazor WebAssembly, Bootstrap 5 dark theme (`data-bs-theme="dark"`). Desktop-first with 240px sticky sidebar (`Layout/MainLayout.razor`). Nav sections: Overview, Management, Support, System. Auth via `<AuthorizeView Policy="@AuthorizationPolicies.Administrator">`. Sidebar footer shows username, edit-profile link (Keycloak account page via `OidcSettings.GetEditAccountUrl()`), and logout. Pages: `Pages/Home.razor` (`/dashboard` — two stat cards: Total Files and Storage Used, populated from `GET /storage/statistics` via `IStorageService`), `Pages/HostServices/ManualTriggers.razor`, `Pages/Storages/AssetsOverview.razor` (`/storages/assets` — paged asset table, 20/page, with previous/next pagination), `Pages/Audits/AuditLogs.razor` (`/audits/{EntityId:guid}` — all audit log entries for a single entity). The Management nav "Assets" link points to `/storages/assets`. `IAssetService` (shared) exposes `GetAssetsMetadataAsync(int page)` and `DeleteAssetContentAsync(Guid id)` for admin use; `IStorageService` (shared) exposes `GetStorageStatisticsAsync()` for the dashboard; `IAuditService` (shared) exposes `GetAuditLogsAsync(Guid entityId)` for the audit log page; `MadWorldEU.Byakko.Services`, `MadWorldEU.Byakko.Storages`, `MadWorldEU.Byakko.Audits`, `MadWorldEU.Byakko.Audits.Summaries`, and `MadWorldEU.Byakko.Formatters` are global usings in `_Imports.razor`.

**AssetsOverview features:**
- Each row has an info icon (Bootstrap Icons info-circle SVG) next to the filename; hovering shows a Bootstrap tooltip (`data-bs-html="true"`) with file size and last updated date on separate lines. Tooltip initialization via `initTooltips()` in `wwwroot/js/app.js` (loaded in `index.html`), called from `OnAfterRenderAsync`.
- Each asset row has a **Logs** button (links to `/audits/{id}`). Each non-deleted asset row also has a **Delete** button; clicking it shows an inline confirm prompt ("Delete content? Yes / No") in the same row. The **Logs** button is hidden while the confirm prompt is visible. Confirming calls `DeleteAssetContentAsync` and reloads the page.
- `AssetMetadataResponse` (in `Core.Contracts`) includes `IsDeleted` and `Size` (bytes as `long`) in addition to the base fields.

**AuditLogs page** (`Pages/Audits/AuditLogs.razor`, route `/audits/{EntityId:guid}`): Shows a table of audit log entries (Entity Type badge, Action badge, IP Address, User ID, Occurred At) for the given entity. Requires `Administrator` policy. Back link to `/storages/assets`.

## Portal UI

**Logo/favicon:** `wwwroot/icons/logo.svg` — Bootstrap Icons `cloud-download`, fill `#0d6efd`. Used in navbar brand (28×28) via `<img>`. Favicon wired in `wwwroot/index.html` as `<link rel="icon" type="image/svg+xml" href="/icons/logo.svg" />`.

Blazor WebAssembly, Bootstrap 5 dark theme. Sticky top navbar with auth dropdown (username, edit-profile link via `OidcSettings.GetEditAccountUrl()`, logout) and a "My files" link (visible only to users satisfying the `User` policy, via a nested `<AuthorizeView Policy="@AuthorizationPolicies.User">`). Pages: `Pages/Home.razor` (`/` — landing page with beta banner, hero section, feature cards, and how-it-works steps), `Pages/Storage/Upload.razor` (`/storage/upload`), `Pages/Storage/Download.razor` (shows expiry warning client-side; server enforces; see below), `Pages/Storage/MyAssets.razor` (`/storage/my-assets` — paged table of the authenticated user's own assets with status badges Active/Expired/Deleted, size, expiry, and download links; fetches from `GET /assets/me`), `Pages/UserAgreement.razor` (`/user-agreement` — static page with 9 sections covering acceptance, prohibited content, file retention, privacy, liability, changes, and contact). Max upload size from `IOptions<AssetSettings>` → `IBrowserFile.OpenReadStream(maxAllowedSize)`. Footer (`Layout/MainLayout.razor`) includes a "User Agreement" link to `/user-agreement`.

**Upload page** (`Pages/Storage/Upload.razor`, route `/storage/upload`): File picker + optional "Advanced options" section (collapsed by default) containing a password input with a show/hide toggle and a **Generate** button. Clicking Generate calls `GeneratePasswordUseCase.Execute()` (injected; registered as scoped in `AddByakkoServices`) and fills the input, automatically switching it to visible. On upload, calls `POST /assets` to create metadata then `PUT /assets/{id}/content` (multipart form) with the file and an optional `password` field. Password is sent as `null` when the field is left empty. `MadWorldEU.Byakko.Application` is a global using in `_Imports.razor` so `GeneratePasswordUseCase` is available without an explicit `@using`.

**Download page** (`Pages/Storage/Download.razor`, route `/storage/download/{Id}`): Loads asset metadata on init. Shows filename, content type, and expiry. Always shows an optional password input (leave empty if file was not password-protected). Download button calls `window.downloadFileWithPassword(url, password, dotNetRef)` in `wwwroot/js/app.js`, which does a JS `fetch` POST to `POST /assets/{id}/content` with the password in a JSON body. The response body is read as a `ReadableStream` chunk-by-chunk; on each chunk `dotNetRef.invokeMethodAsync('UpdateProgress', percent)` is called to update the in-page progress bar (indeterminate when `Content-Length` is absent). On completion, a blob URL is created and a temporary `<a download>` is clicked. On error, the `FailureResponse.code` is checked: `Encryption.DecryptionFailed` → "The password is incorrect."; otherwise `description` is shown. The component implements `IAsyncDisposable` to release the `DotNetObjectReference`.

**Beta banner:** A full-width amber `alert-warning` strip sits above the hero section on `Pages/Home.razor`, informing visitors that the service is in beta and new accounts require manual admin approval.

**Portal components** (`Components/`): `UploadLimitsAlert.razor` — reusable alert showing max file size and files remaining (amber when limit reached); used in `Upload.razor` and `MyAssets.razor`. Takes a `GetUserUploadLimitsResponse?` parameter. Namespace `MadWorldEU.Byakko.Components` is a global using in `_Imports.razor`.

`IAssetService` (shared) additionally exposes `GetMyAssetsMetadataAsync(int page)` — calls `GET /assets/me?page={page}` with the authorized client.

**Localization:** The Portal supports English (`en`), Dutch (`nl-NL`), and Japanese (`ja-JP`). Key pieces:

- `BlazorWebAssemblyLoadAllGlobalizationData=true` in `Portal.csproj` — required for runtime culture switching.
- `Localization/Resources.resx` (default/English), `Resources.nl-NL.resx`, `Resources.ja-JP.resx` — all user-visible strings. All pages inject `@inject IStringLocalizer<Resources> Localizer` and use `@Localizer["Key"]`.
- `Localization/SupportedLanguages.cs` — static class listing the three supported culture codes with their display labels (`EN`/`NL`/`JA`) and flag emojis (🇬🇧/🇳🇱/🇯🇵). Used by `CultureResolver` and `MainLayout`.
- `Localization/CultureResolver.cs` — resolves the active culture from a saved localStorage preference; falls back to the browser language (`navigator.language`) with neutral-code matching (`nl-BE` → `nl-NL`); falls back to `en` if no match.
- `Program.cs` — after `host.Build()`, reads `getCulture` and `getBrowserLanguage` via JS interop, resolves the culture via `CultureResolver.Resolve`, and sets `CultureInfo.DefaultThreadCurrentCulture/UICulture` before `RunAsync()`.
- `wwwroot/js/app.js` — `getCulture()` / `setCulture(culture)` read/write `byakko-culture` in localStorage; `getBrowserLanguage()` returns `navigator.language`.
- `Layout/MainLayout.razor` — navbar language switcher: a flag dropdown on desktop; on mobile the current flag is always visible next to the hamburger, and the expanded menu shows all three flags inline. Selecting a language calls `setCulture` and reloads with `forceLoad: true`.
- Shared pages (`NotFound.razor`, `AccessDenied.razor`, `TestPage.razor` in `Controller.Blazor.Shared`) use `IStringLocalizer<SharedResources>` backed by `Localization/SharedResources.resx` / `SharedResources.nl-NL.resx` / `SharedResources.ja-JP.resx` in `Blazor.Shared`. Admin always uses English because it never sets the culture. `AddLocalization()` is registered in `AddByakkoServices()` (shared startup extension), so it applies to both Portal and Admin.

## Status UI

**Logo/favicon:** `wwwroot/icons/logo.svg` — Bootstrap Icons `activity` (pulse line), fill `#0d6efd`. Used in sidebar header (22×22) and mobile header (18×18) via `<img>`. Favicon wired in `Components/App.razor` as `<link rel="icon" type="image/svg+xml" href="/icons/logo.svg" />`.

Blazor Server (static SSR), Bootstrap 5 dark theme (`data-bs-theme="dark"`). Public — no authentication. 240px sticky sidebar (`Layout/MainLayout.razor`) with `status-sidebar` / `status-nav-link` CSS classes (`wwwroot/app.css`). Single page: `Components/Pages/Home.razor` (`/`) — Bootstrap card grid showing health status for 6 services.

Health checks wired via `GetHealthServicesUseCase` (`Application/Healths/`) — runs all 6 checks in parallel with a 2-second timeout each:
- **API, Portal, Admin, Authentication** — HTTP GET to configurable URLs (`HealthChecks:*` config keys); 200 = Healthy, non-200 = Unhealthy, body `"Degraded"` = Degraded, empty URL = Unknown.
- **Database** — `ByakkoContext.Database.CanConnectAsync()`.
- **Object Storage** — `IAmazonS3.ListBucketsAsync()` with `CancellationTokenSource` timeout.

Config: `HealthChecks:Api/Admin/Portal/Authentication` URLs; `ConnectionStrings:byakko-db`; `ConnectionStrings:localstack`; `Storage:*` (same keys as API). Liveness/readiness probe at `/health`.

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
- **Shared services:** `IAssetService`/`AssetService`, `IStorageService`/`StorageService`, and `IAuditService`/`AuditService` live in `Controller.Blazor.Shared/Services/` and are registered in `WebAssemblyHostBuilderExtensions.AddByakkoServices()`. All methods that call the API return `ResultResponse<T>` (see below).
- **Shared formatters:** `ByteFormatter.Format(long?)` in `Controller.Blazor.Shared/Formatters/ByteFormatter.cs` — formats byte counts to B/KB/MB/GB using `InvariantCulture` (always `.` as decimal separator).
- **appsettings schemas:** each `appsettings.json` has a companion `appsettings-schema.json`; update schema when adding config keys
- **EF Core constructor:** `private`, `[UsedImplicitly]`, XML `<summary>` saying "Required for EF Core"
- **Errors:** `static readonly` fields on `{Domain}Errors` class; code format `"Domain.Reason"`

### Blazor HTTP Response Pattern

All Blazor service methods that call the API return `ResultResponse<T>` (`Controller.Blazor.Shared/Responses/ResultResponse.cs`, namespace `MadWorldEU.Byakko.Responses`). DELETE endpoints that return no body use `ResultResponse<EmptyResponse>`.

**Types in `Core.Contracts/Common/` (namespace `MadWorldEU.Byakko.Common`):**

| Type | Purpose |
|---|---|
| `FailureResponse` | Structured API error — `Code` (`Domain.Reason`), `StatusCode` (HTTP), `Description` (human-readable) |
| `EmptyResponse` | Placeholder success body for endpoints that return no content (e.g. DELETE) |

**`ResultResponse<T>` API:**
- `IsSuccess` — `true` when `Failure == null`
- `Response` — the success payload (`T?`)
- `Failure` — the `FailureResponse` on error
- Implicit operators from `T` and `FailureResponse` allow returning either directly

**`HttpClientExtensions` (`Controller.Blazor.Shared/Responses/`, namespace `MadWorldEU.Byakko.Responses`):**

| Method | HTTP verb | Notes |
|---|---|---|
| `GetResultResponseFromJsonAsync<TResponse>` | GET | Reads JSON body on success |
| `PostResultResponseFromJsonAsync<TRequest, TResponse>` | POST | Serialises request as JSON |
| `PutResultResponseFromJsonAsync<TResponse>` | PUT | Accepts raw `HttpContent` (e.g. `MultipartFormDataContent`) |
| `DeleteResultResponseFromJsonAsync` | DELETE | Returns `ResultResponse<EmptyResponse>` |

**Usage pattern in Razor pages:**
```csharp
var result = await SomeService.SomeMethodAsync();

if (result.IsSuccess)
{
    _data = result.Response;
}
else
{
    _errorMessage = ErrorTranslator.Translate(result.Failure!);
}
```

### Error Translation

**`IErrorTranslator` / `ErrorTranslator`** (`Controller.Blazor.Shared/Localization/`, namespace `MadWorldEU.Byakko.Localization`): Translates an error code to a localized user-facing message. Registered in `AddByakkoServices()`.

| Method | Notes |
|---|---|
| `Translate(FailureResponse)` | Looks up `response.Code` in `ErrorResources`; falls back to `response.Description` |
| `Translate(string code, string defaultDescription)` | Looks up `code`; falls back to `defaultDescription` |

**`ErrorResources`** (`Controller.Blazor.Shared/Localization/ErrorResources.resx` + `.nl-nl.resx` + `.ja-jp.resx`): Localized strings keyed by error code (e.g. `"Asset.NotFound"`). Contains translations for all domain error codes and all `BlazorErrors` codes. Add a new entry to all three files whenever a new error code is introduced.

**`BlazorErrors`** (`Controller.Blazor.Shared/Configurations/BlazorErrors.cs`, namespace `MadWorldEU.Byakko.Configurations`): Client-side errors not tied to an API `FailureResponse`. Each entry is a `BlazorError` with a `Code` and fallback `Description`.

| Field | Code | When to use |
|---|---|---|
| `FileLoadFailed` | `Blazor.File.LoadFailed` | `catch` block when loading a single file fails |
| `FilesLoadFailed` | `Blazor.Files.LoadFailed` | `catch` block when loading a list of files fails |
| `FileDeleteFailed` | `Blazor.File.DeleteFailed` | `catch` block when deleting a file fails |
| `FileUploadFailed` | `Blazor.File.UploadFailed` | `catch` block when uploading a file fails |
| `FileInvalidId` | `Blazor.File.InvalidId` | Invalid or unparseable asset ID in the URL |
| `AuditLogsLoadFailed` | `Blazor.AuditLogs.LoadFailed` | `catch` block when loading audit logs fails |
| `TriggerFailed` | `Blazor.Trigger.Failed` | `catch` block when a manual trigger request fails |

**Error handling pattern in `catch` blocks** — always log the real exception to the browser console, show only the localized generic message to the user, and use `_errorMessage` (not `_error`) as the variable name:
```csharp
catch (Exception ex)
{
    await JsRuntime.InvokeVoidAsync("console.error", ex.ToString());
    _errorMessage = ErrorTranslator.Translate(BlazorErrors.FileLoadFailed.Code, BlazorErrors.FileLoadFailed.Description);
}
```

## Helm / Kubernetes

Chart in `deployments/helm/byakko/`. See `docs/developer-guides/Kubernetes.md` for setup.

Key templates: `api.yaml`, `portal.yaml`, `admin.yaml`, `status.yaml`, `database.yaml`, `keycloak.yaml`, `keycloak-database.yaml`, `pgadmin.yaml`, `localstack.yaml` (guarded by `localstack.enabled`), observability stack (`otel-collector`, `prometheus`, `tempo`, `loki`, `grafana` — guarded by `observability.enabled`), `headlamp.yaml` (guarded by `headlamp.enabled`), `ingress.yaml`, `middlewares.yaml`.

Subdomains: `byakko.dev`/`www.`/`fileshare.` → Portal; `api.` → API; `admin.` → Admin; `status.` → Status; `database.` → pgAdmin; `authentication.` → Keycloak; `grafana.` → Grafana; `kubernetes.` → Headlamp (guarded by `headlamp.enabled`).

**Headlamp**: `headlamp.yaml` creates an `ExternalName` Service in the byakko namespace pointing to `my-headlamp.kube-system.svc.cluster.local` (Headlamp must be pre-installed in `kube-system` via its own Helm chart). The ingress routes `kubernetes.<domain>` to this service via a dedicated `byakko-ingress-headlamp` — intentionally separate from `byakko-ingress` to avoid the `security-headers` middleware (COOP/COEP headers break the OIDC redirect flow). OIDC login via Keycloak requires: (1) a `headlamp-client` in Keycloak, (2) Headlamp upgraded with OIDC args, (3) the microk8s kube-apiserver configured with `--oidc-issuer-url`, `--oidc-client-id`, `--oidc-username-claim=email`, and `--oidc-groups-claim=groups` in `/var/snap/microk8s/<version>/args/kube-apiserver`, and (4) a `ClusterRoleBinding` mapping the user's Keycloak email to a Kubernetes role. See `docs/developer-guides/Kubernetes.md` Step 6 for the full setup.

All app image tags are controlled by a single top-level `appVersion` in `values.yaml` (set via `--set appVersion` in the pipeline). Storage config (`Storage:Mode`, `BucketName`, `OvhCloud.*`) lives under the top-level `storage:` key, shared by both `api.yaml` and `status.yaml`.

**Grafana Keycloak authentication:** Grafana authenticates against Keycloak via Generic OAuth. Configured entirely through env vars in `grafana.yaml` (no grafana.ini). Key env vars: `GF_SERVER_ROOT_URL` (must be set so Grafana constructs the correct `redirect_uri`), `GF_AUTH_GENERIC_OAUTH_*`. The client secret is stored in the Grafana Secret and loaded via `envFrom`. Role mapping: `contains(roles, 'Administrator') && 'Admin' || ''` with `roleAttributeStrict=true` — only Keycloak users with the `Administrator` realm role can sign in. **Critical Keycloak requirement:** The realm roles mapper (`Client Scopes → roles → Mappers → realm roles`) must have **Add to ID token** enabled. Grafana calls the userinfo endpoint via the external hostname but Keycloak rejects the access token with 401 because the `iss` in the token (`https://authentication.<domain>/...`) doesn't match the request URL — Grafana falls back to the ID token only, so roles must be in the ID token. The `grafana-client` setup steps are in `docs/developer-guides/AuthenticationServer.md`. The client secret is injected via `GRAFANA_KEYCLOAK_CLIENT_SECRET` in the deployment pipeline (see `docs/developer-guides/Pipelines.md`).

**pgAdmin Keycloak authentication:** pgAdmin authenticates against Keycloak via OAuth2. Configured via a `config_local.py` mounted from the `pgadmin-oauth` ConfigMap at `/pgadmin4/config_local.py`. `AUTHENTICATION_SOURCES = ['oauth2']` disables the built-in login form entirely — only Keycloak is accepted. `OAUTH2_SERVER_METADATA_URL` must be set to Keycloak's `/.well-known/openid-configuration` endpoint — pgAdmin requires it to verify the ID token when `openid` is in the scope. The client secret is read from the `PGADMIN_KEYCLOAK_CLIENT_SECRET` env var (injected from the pgAdmin Secret via `envFrom`). Access is restricted to `Administrator` realm-role users by binding the `administrator-only-browser` Keycloak flow to `pgadmin-client` — non-admins are rejected at Keycloak before reaching pgAdmin. The `pgadmin-client` setup steps are in `docs/developer-guides/AuthenticationServer.md`. The client secret is injected via `PGADMIN_KEYCLOAK_CLIENT_SECRET` in the deployment pipeline (see `docs/developer-guides/Pipelines.md`). Both PostgreSQL servers (Byakko and Keycloak) are pre-configured via `servers.json` with `"Shared": true` so all authenticated users see them. Note: pgAdmin does not share credentials for shared servers — each user must enter their own username and password on first connect (host, port, and database are pre-filled).

**Grafana dashboard provisioning:** Dashboards are stored as JSON files in `deployments/helm/byakko/grafana/dashboards/`. `grafana.yaml` reads them via `.Files.Glob "grafana/dashboards/*.json"` into the `grafana-dashboards` ConfigMap, mounts it at `/var/lib/grafana/dashboards`, and the provisioner (`updateIntervalSeconds: 30`) picks them up automatically on `helm upgrade` — no manual import needed. The Loki logging dashboard (`loki-logging.json`) uses `service_name` and `k8s_pod_name` stream labels and a `source` variable backed by `log_source` to distinguish log origins: `application` = OTLP logs from the API/Status apps, `k8s_pods` = raw pod stdout/stderr scraped by the DaemonSet. Datasource names in dashboard JSON must match the provisioned names exactly (`"Loki"`, `"Prometheus"`, `"Tempo"`) — template variable placeholders (`${DS_LOKI}`) are only resolved on manual UI import, not file provisioning. Stream selector matchers must use `.+` not `.*` to avoid Loki's empty-compatible value rejection.

**Observability data retention (60 days):** All three stores are configured to auto-purge data older than 60 days. Loki: `limits_config.retention_period: 1440h` + `compactor.retention_enabled: true` in `loki.yaml`. Prometheus: `--storage.tsdb.retention.time=60d` CLI flag in `prometheus.yaml`. Tempo: `compactor.compaction.block_retention: 1440h` in `tempo.yaml`.

TLS local: `mkcert byakko.dev "*.byakko.dev"` + `kubectl create secret tls`. TLS production: `clusterIssuer.enabled = true` + cert-manager HTTP-01. Remove AAAA records if no IPv6.

## Documentation & Diagrams

DocFX site at `https://madworldeu.github.io/Byakko`. Preview: `docfx docs/docfx.json --serve`.

C4 diagrams in `docs/diagrams/workspace.dsl` (Structurizr DSL). Edit locally:
```bash
docker run -it --rm -p 8080:8080 -v ./:/usr/local/structurizr structurizr/structurizr local
```