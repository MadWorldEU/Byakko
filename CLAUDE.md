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

No tests exist yet. The solution file has placeholder folders `/tests/controller/` and `/tests/core/` for future test projects.

```bash
dotnet test                           # Run all tests (once added)
```

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