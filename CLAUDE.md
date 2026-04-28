# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
dotnet build                          # Build all projects
dotnet run --project src/MadWorldEU.Byakko.Controller.Api/Api.csproj
dotnet watch --project src/MadWorldEU.Byakko.Controller.Api/Api.csproj
```

The API runs on `http://localhost:5062` (HTTP) or `https://localhost:7286` (HTTPS).
OpenAPI spec is available at `/openapi/v1.json` in the Development environment.

## Testing

Three test projects exist:

| Project | Framework | Purpose |
|---|---|---|
| `MadWorldEU.Byakko.Controller.Api.IntegrationTests` | Reqnroll + TUnit | BDD integration tests for the API |
| `MadWorldEU.Byakko.Core.Application.Unittests` | TUnit + Shouldly | Unit tests for application logic |
| `MadWorldEU.Byakko.Controller.Admin.IntegrationTests` | — | Placeholder |
| `MadWorldEU.Byakko.Controller.Portal.IntegrationTests` | — | Placeholder |

```bash
dotnet test                                    # Run all tests
dotnet test tests/MadWorldEU.Byakko.Controller.Api.IntegrationTests/ -- --coverage  # With code coverage
```

**API integration tests** use `WebApplicationFactory<Program>` (in-process test server). The hook in `Hooks/ApiHooks.cs` spins up the factory once per test run and injects an `HttpClient` into each scenario via `ScenarioContext`. BDD scenarios live in `Features/` as `.feature` files; step definitions live in `StepDefinitions/`.

## Architecture

Clean Architecture with four layers:

| Layer | Project | Role |
|---|---|---|
| Controller | `MadWorldEU.Byakko.Controller.Api` | ASP.NET Core Minimal APIs |
| Application | `MadWorldEU.Byakko.Core.Application` | Business logic / use cases |
| Domain | `MadWorldEU.Byakko.Core.Domain` | Domain models / entities |
| Infrastructure | `MadWorldEU.Byakko.Infrastructure.Postgresql` | PostgreSQL data access |

Dependencies flow inward: Controller → Application → Domain ← Infrastructure.

## Project Conventions

- **Target framework:** .NET 10.0
- **Nullable reference types** and **implicit usings** are enabled globally via `Directory.Build.props`
- **`TreatWarningsAsErrors = true`** — all warnings must be resolved
- **Central Package Management** — NuGet versions are defined only in `Directory.Packages.props`, not in individual `.csproj` files
- **Root namespace:** `MadWorldEU.Byakko`
- **Solution format:** `.slnx` (new Visual Studio format)
- **Indentation:** 4 spaces, LF line endings, UTF-8 (enforced by `.editorconfig`)
- **`var` usage:** preferred in all contexts (enforced by `.editorconfig`)