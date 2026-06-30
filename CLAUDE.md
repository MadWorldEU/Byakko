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

Build from the **repository root** with `-f src/MadWorldEU.Byakko.Controller.<Name>/Dockerfile .` (Api, Portal, Admin, Status). Portal/Admin: nginx:alpine port 8080 (non-root). Status: aspnet:10.0. Shared nginx: `Controller.Blazor.Shared/DockerConfigs/nginx.conf`.

**Portal entrypoint:** `Controller.Portal/Docker/docker-entrypoint.sh` runs `envsubst '${OG_IMAGE_URL}'` on `index.html` — only that variable is substituted, leaving all other `${}` content untouched.

## Architecture

Clean Architecture. Dependencies flow inward: Controller → Application → Domain ← Infrastructure.

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
| Infrastructure | `Infrastructure.Mail` | SMTP mail via MailKit |
| Host | `Aspire` | .NET Aspire AppHost |

Dependency rules: BuildingBlocks has none. Domain must not depend on Application/Contracts/Infrastructure/Controllers. Application must not depend on Infrastructure/Controllers. Infrastructure must not depend on Controllers. Blazor projects may only depend on Contracts and Blazor.Shared.

See `.claude/rules/architecture.md` for DDD building blocks and functional patterns.

## Rules

- **Testing:** `.claude/rules/testing.md`
- **Code standards:** `.claude/rules/code-standard.md`
- **APIs:** `.claude/rules/api.md`
- **Infrastructure & config:** `.claude/rules/infrastructure.md`
- **UI (Admin, Portal, Status):** `.claude/rules/ui.md`
- **Helm / Kubernetes:** `.claude/rules/k8s.md`

## Documentation & Diagrams

DocFX: `docfx docs/docfx.json --serve` → `https://madworldeu.github.io/Byakko`.

C4 diagrams: `docs/diagrams/workspace.dsl` — `docker run -it --rm -p 8080:8080 -v ./:/usr/local/structurizr structurizr/structurizr local`.
