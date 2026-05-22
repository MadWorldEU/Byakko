# Testing

These rules define the testing standards for this project. All test code must follow these conventions to ensure reliability, consistency, and maintainability across the test suite.

## Project Location

All test projects must be placed in the `tests/` folder at the repository root. Never add test projects inside `src/`.

## Technology

| Library | Used in | Purpose |
|---|---|---|
| TUnit | All test projects | Test runner and assertions framework |
| Shouldly | All test projects | Fluent assertion syntax |
| Reqnroll.TUnit | `Api.IntegrationTests` | BDD (Gherkin) scenario support; `.feature` files + step definitions |
| Microsoft.AspNetCore.Mvc.Testing | `Api.IntegrationTests` | In-process test server via `WebApplicationFactory<Program>` |
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