# Testing

These rules define the testing standards for this project. All test code must follow these conventions to ensure reliability, consistency, and maintainability across the test suite.

## Project Location

All test projects must be placed in the `tests/` folder at the repository root. Never add test projects inside `src/`.

## Test Projects

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

Key notes:
- API integration tests use real PostgreSQL + LocalStack + Keycloak Testcontainers — no mocks. `Authentication:ValidateUser = false` for self-signed tokens.
- Status integration tests use real PostgreSQL + LocalStack + Mailpit Testcontainers; WireMock.Net stubs the 4 HTTP health endpoints returning `200 Healthy`. `MAILPIT_HOST`/`MAILPIT_PORT` injected via in-memory config.
- Application unit tests cover error paths only. Use `Result.Failure<T>(error)` not `Result<T>.Failure(error)`.
- Domain unit tests use a `BuildAsset()` helper for constructing valid aggregates.
- Architecture tests: BDD feature files + `BaseArchitectureTests`; every assembly needs a marker interface (e.g. `IPostgresqlMarker`) in its root namespace.
- Account integration tests (`Features/Accounts/Accounts.feature`) use a `[BeforeScenario(Order=2)]` in `AccountsSteps` that creates a fresh `HttpClient` with a unique `Guid` user ID per scenario — stored under both the default `HttpClient` key and `ScenarioContextKeys.AuthenticatedClient` so the shared "Given I am authenticated as a user" step still works. `ApiHooks.BeforeScenario` runs at `Order=1` and stores the `WebApplicationFactory<Program>` under `ScenarioContextKeys.Factory` for this purpose. The unique user ID is stored under `ScenarioContextKeys.AccountUserId` so it can be shared across step definition classes.
- **Keycloak test setup** (`ApiHooks.BeforeTestRun`): starts a `KeycloakContainer` (image `quay.io/keycloak/keycloak:26.0.7`), creates the `MadWorld` realm with a minimal representation, then creates a `madworld-admin-api` confidential client in the master realm with service accounts enabled and `manage-users` granted on the `MadWorld-realm` client. `KeyCloak__*` config is injected into the `WebApplicationFactory` so `IAuthenticationRepository` talks to the real test container. `KeycloakAdminTestClient` (`Common/`) is a helper that uses admin-cli password grant to drive the Keycloak Admin REST API from step definitions; it is stored per-scenario under `ScenarioContextKeys.KeycloakAdmin`. Do **not** POST the full realm export JSON to `/admin/realms` — Keycloak rejects it via REST API due to hashed secrets; create a minimal realm instead.

## Technology

| Library | Used in | Purpose |
|---|---|---|
| TUnit | All test projects | Test runner and assertions framework |
| Shouldly | All test projects | Fluent assertion syntax |
| Reqnroll.TUnit | `Api.IntegrationTests` | BDD (Gherkin) scenario support; `.feature` files + step definitions |
| Microsoft.AspNetCore.Mvc.Testing | `Api.IntegrationTests` | In-process test server via `WebApplicationFactory<Program>` |
| Testcontainers.Keycloak | `Api.IntegrationTests` | Real Keycloak instance spun up per test run |
| Testcontainers.PostgreSql | `Api.IntegrationTests` | Real PostgreSQL instance spun up per test run |
| Testcontainers.Minio | `Api.IntegrationTests` | Real MinIO (S3-compatible) instance spun up per test run |
| Microsoft.Testing.Extensions.CodeCoverage | `Api.IntegrationTests` | Code coverage collection |
| bunit | `Portal.Componenttests`, `Admin.Componenttests` | Blazor component rendering in-process |
| WireMock.Net | `Portal.Componenttests`, `Admin.Componenttests` | HTTP server mock for faking API responses |

TUnit test attributes (`[Test]`, `[Before]`, `[After]`, etc.) are available without explicit usings via source generator — no `using TUnit;` needed.

## Component Tests

Component tests live in `Portal.Componenttests` and `Admin.Componenttests`. They render Blazor components in-process using bUnit's `TestContext` and mock HTTP calls with a `WireMockServer`.

**Naming:** `[MethodUnderTest]_When[Condition]_Should[ExpectedResult]` — for example `OnInitializedAsync_WhenAssetExists_ShouldShowFileMetadataAndDownloadLink`.

**Structure per test:**
1. Start a `WireMockServer` and register the expected HTTP stubs.
2. Create a `BunitContext`, register named `HttpClient`s pointing at the WireMock URL, and register any required services.
3. Render the component under test via `ctx.Render<T>`.
4. Call `cut.WaitForState(predicate, timeout)` to block until async initialisation completes.
5. Assert on the rendered markup using CSS selectors (`cut.Find`, `cut.FindAll`).
6. Dispose both `WireMockServer` and `TestContext` via `using` — they must not outlive the test method.

Use `BunitContext` (not the obsolete `TestContext`) and `Render<T>` (not the obsolete `RenderComponent<T>`).

## Test Isolation

Each test must be fully independent. Tests must not rely on execution order, share mutable state, or assume anything left behind by a previous test. Every scenario must arrange its own data and clean it up. If a test fails, it must not cause other tests to fail.

## Resource Cleanup

Always release resources after tests complete. Use `[AfterEach]` or `[AfterAll]` hooks to dispose containers, connections, and any other external resources. Implement `IAsyncDisposable` or `IDisposable` on test fixtures that hold resources. Never rely on garbage collection to clean up — unDisposed resources cause port conflicts and flaky tests in subsequent runs.