# API

## Assets

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

**Manual cleanup triggers** (`ManualTriggersEndpoints.cs`, `Administrator` policy):
- `POST /host-services/manual-triggers/clean-up/assets-content` → `DeleteAllExpiredContentOfAssetsUseCase`
- `POST /host-services/manual-triggers/clean-up/assets-metadata` → also bulk-deletes matching audit logs
- `POST /host-services/manual-triggers/clean-up/accounts` → `DeleteRequestedAccountsUseCase`

**Audit log domain** (`Core.Domain/Audits/`): `AuditLog` entity with `Id`, `EntityId`, `EntityType`, `Action`, `IpAddress`, `OccurredAt` (`Instant`), `OccurredBy`. `IpAddress.Create(null/empty)` → `AuditErrors.InvalidIpAddress`. Audit log deletion for expired assets is handled in `AssetRepository.DeleteExpiredAssets`, not via `IAuditRepository`.

**AuditAssetsHandler** handles `AssetMetaDataCreatedEvent` and `AssetContentUploadedEvent`; audit creation failure is non-fatal (logs warning, does not throw).

**AuditErrors:** `InvalidIpAddress`, `SaveFailed`, `QueryFailed`, `DeleteFailed`. `IAuditRepository.DeleteAsync(UserId)` removes all audit log entries for a user.

## Accounts

Endpoints in `Controller.Api/Endpoints/Accounts/AccountsEndpoints.cs`.

| Method | Route | Use case | Policy | Notes |
|---|---|---|---|---|
| `GET` | `/accounts/` | `GetAccountsPendingDeletionUseCase` | `Administrator` | Paged (20/page); query param `page`; returns `DeletionRequested` + `DeletionConfirmed` accounts |
| `POST` | `/accounts/{userId}/cancel-deletion-request` | `CancelDeletionRequestAccountUseCase` | `Administrator` | 404 not found, 409 `DeletionNotRequested` |
| `POST` | `/accounts/{userId}/confirm-deletion` | `ConfirmDeletionAccountUseCase` | `Administrator` | 404 not found, 409 `DeletionNotRequested` |
| `POST` | `/accounts/me` | `CreateMyAccountUseCase` | `User` | 201 Created; 400 on failure |
| `GET` | `/accounts/me` | `GetMyAccountUseCase` | `User` | 404 not found; returns `UserId` + `Status` (string) |
| `POST` | `/accounts/me/deletion-request` | `RequestDeletionMyAccountUseCase` | `User` | 404 not found, 409 `NotActive` |

**Account domain** (`Core.Domain/Accounts/`): `Account` entity with `UserId`, `Status` (`AccountStatus`), `CreatedAt`, `UpdatedAt`. `Create(clock, guidGenerator, userId)` → stamps both timestamps. `RequestDeletion(clock)` → `AccountErrors.NotActive` if `Status != Active`; sets `Status = DeletionRequested`. `CancelDeletionRequest(clock)` → `AccountErrors.DeletionNotRequested` if `Status != DeletionRequested`; sets `Status = Active`. `ConfirmDeletion(clock)` → `AccountErrors.DeletionNotRequested` if `Status != DeletionRequested`; sets `Status = DeletionConfirmed`. `Delete(clock)` → `AccountErrors.DeletionNotConfirmed` if `Status != DeletionConfirmed`; sets `Status = Deleted`; dispatches `AccountDeletedEvent`.

**AccountStatus** enum (`Core.Domain/Accounts/AccountStatus.cs`): `Active` → `DeletionRequested` → `DeletionConfirmed` → `Deleted`. `Deleted` is terminal. Stored as `string` via EF Core `HasConversion<string>()`. Exposed as `string Status` in response DTOs.

**AccountErrors:** `NotFound`, `QueryFailed`, `SaveFailed`, `UpdateFailed`, `NotActive`, `DeletionAlreadyRequested`, `DeletionNotRequested`, `DeletionNotConfirmed`.

**AccountRepository** (`Infrastructure.Postgresql/Accounts/`): `FindAsync(UserId)`, `AddAsync(Account)`, `UpdateAsync(Account)`, `UpdateAsync(Account)`, `DeleteAsync(UserId)` (hard-delete), `GetAccountsPendingDeletion(Page)`, `GetConfirmedDeletionAccounts()`. `UserId` has a unique index via `AccountEntityTypeConfiguration`.

**AccountDeletedEvent** domain event dispatched by `DeleteRequestedAccountsUseCase` after each account is marked `Deleted`. Two handlers:
- `AuditAccountEventHandler` — calls `IAuditRepository.DeleteAsync(UserId)` to remove all audit log entries for the user.
- `AssetAccountEventHandler` — fetches all assets via `IAssetRepository.GetAssetsAsync(UserId)`, deletes each file from object storage via `IContentStorage.DeleteAsync`, then hard-deletes all records via `IAssetRepository.DeleteAsync(UserId)`. Content deletion failures are logged as warnings and do not abort the remaining assets.

**DeleteRequestedAccountsUseCase** (`Core.Application/Accounts/`): fetches all `DeletionConfirmed` accounts, calls `Delete(clock)` on each, persists via `UpdateAsync`, and dispatches `AccountDeletedEvent`. Per-account failures are logged as warnings and do not abort remaining accounts.

**IAssetRepository** additions: `GetAssetsAsync(UserId)` returns all assets for a user; `DeleteAsync(UserId)` hard-deletes all asset records for a user; `DeleteAsync(Asset)` hard-deletes a single asset record.

## Correspondences

`POST /correspondences/feedback` → `SendFeedbackUseCase`. Public; rate-limited `PublicPost` (1 req/60s per IP). `SendFeedbackRequest` carries `Email` + `Message`. `Email` value object: empty → `EmailErrors.Empty`; invalid format → `EmailErrors.Invalid`.

## Audits

`GET /audits/{entityId}` → `GetAuditLogsUseCase`. `Administrator` policy. Returns `GetAuditLogsResponse` with `IReadOnlyList<AuditLogResponse>` (Id, EntityType, Action, IpAddress, OccurredAt, OccurredByUserId).

## Storage Statistics

`GET /storage/statistics` → `GetStorageStatisticsUseCase`. `Administrator` policy. Returns `TotalFiles` (`int`) + `TotalBytes` (`long`) for non-deleted assets. Uses `EF.Property<long>` + `Select` to work around EF Core's inability to translate `.Value` on a value-object in `SumAsync`.