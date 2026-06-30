# Infrastructure & Config

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