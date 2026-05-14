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

TUnit test attributes (`[Test]`, `[Before]`, `[After]`, etc.) are available without explicit usings via source generator — no `using TUnit;` needed.

## Test Isolation

Each test must be fully independent. Tests must not rely on execution order, share mutable state, or assume anything left behind by a previous test. Every scenario must arrange its own data and clean it up. If a test fails, it must not cause other tests to fail.

## Resource Cleanup

Always release resources after tests complete. Use `[AfterEach]` or `[AfterAll]` hooks to dispose containers, connections, and any other external resources. Implement `IAsyncDisposable` or `IDisposable` on test fixtures that hold resources. Never rely on garbage collection to clean up — unDisposed resources cause port conflicts and flaky tests in subsequent runs.