# UI

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