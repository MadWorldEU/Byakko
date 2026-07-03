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

**Manual cleanup triggers** (`ManualTriggersEndpoints.cs`, requires auth):
- `POST /host-services/manual-triggers/clean-up/assets-content` → `DeleteAllExpiredContentOfAssetsUseCase`
- `POST /host-services/manual-triggers/clean-up/assets-metadata` → also bulk-deletes matching audit logs

**Audit log domain** (`Core.Domain/Audits/`): `AuditLog` entity with `Id`, `EntityId`, `EntityType`, `Action`, `IpAddress`, `OccurredAt` (`Instant`), `OccurredBy`. `IpAddress.Create(null/empty)` → `AuditErrors.InvalidIpAddress`. Audit log deletion for expired assets is handled in `AssetRepository.DeleteExpiredAssets`, not via `IAuditRepository`.

**AuditAssetsHandler** handles `AssetMetaDataCreatedEvent` and `AssetContentUploadedEvent`; audit creation failure is non-fatal (logs warning, does not throw).

## Accounts

Endpoints in `Controller.Api/Endpoints/Accounts/AccountsEndpoints.cs`. All require `User` policy.

| Method | Route | Use case | Notes |
|---|---|---|---|
| `POST` | `/accounts/me` | `CreateMyAccountUseCase` | 201 Created; 400 on failure |
| `GET` | `/accounts/me` | `GetMyAccountUseCase` | 404 not found; returns `UserId` + `HasDeletionRequested` |
| `POST` | `/accounts/me/deletion-request` | `RequestDeletionMyAccountUseCase` | 404 not found, 409 already requested |

**Account domain** (`Core.Domain/Accounts/`): `Account` entity with `UserId`, `HasDeletionRequested`, `CreatedAt`, `UpdatedAt`. `Create(clock, guidGenerator, userId)` → stamps both timestamps. `RequestDeletion(clock)` → `AccountErrors.DeletionAlreadyRequested` if already flagged; sets `HasDeletionRequested = true` and updates `UpdatedAt`.

**AccountErrors:** `NotFound`, `QueryFailed`, `SaveFailed`, `UpdateFailed`, `DeletionAlreadyRequested`.

**AccountRepository** (`Infrastructure.Postgresql/Accounts/`): `FindAsync(UserId)`, `AddAsync(Account)`, `UpdateAsync(Account)`. `UserId` has a unique index via `AccountEntityTypeConfiguration`.

## Correspondences

`POST /correspondences/feedback` → `SendFeedbackUseCase`. Public; rate-limited `PublicPost` (1 req/60s per IP). `SendFeedbackRequest` carries `Email` + `Message`. `Email` value object: empty → `EmailErrors.Empty`; invalid format → `EmailErrors.Invalid`.

## Audits

`GET /audits/{entityId}` → `GetAuditLogsUseCase`. `Administrator` policy. Returns `GetAuditLogsResponse` with `IReadOnlyList<AuditLogResponse>` (Id, EntityType, Action, IpAddress, OccurredAt, OccurredByUserId).

## Storage Statistics

`GET /storage/statistics` → `GetStorageStatisticsUseCase`. `Administrator` policy. Returns `TotalFiles` (`int`) + `TotalBytes` (`long`) for non-deleted assets. Uses `EF.Property<long>` + `Select` to work around EF Core's inability to translate `.Value` on a value-object in `SumAsync`.