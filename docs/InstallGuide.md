# Install Guide

This guide explains how to set up and run Byakko locally for development.

## Requirements

The following tools must be installed before running the application locally.

- **[.NET 10 SDK](https://dotnet.microsoft.com/download)** or higher — runtime and build toolchain
- **[JetBrains Rider](https://www.jetbrains.com/rider/download/)** — recommended IDE (Visual Studio or VS Code also work)
- **[Docker Desktop](https://www.docker.com/products/docker-desktop/)** — required to run PostgreSQL via .NET Aspire

## Running locally with Aspire

.NET Aspire orchestrates the API, Admin, Portal, and PostgreSQL database in a single command. Docker Desktop must be running before you start.

```bash
dotnet run --project src/MadWorldEU.Byakko.Aspire/Aspire.csproj
```

This starts the Aspire dashboard (typically at `http://localhost:15254`) where you can monitor all services, view logs, and inspect traces. The API, Admin, and Portal URLs are listed on the dashboard.

## Running tests locally

```bash
dotnet test                                                                          # Run all tests
dotnet test tests/MadWorldEU.Byakko.Controller.Api.IntegrationTests/               # API integration tests only
dotnet test tests/MadWorldEU.Byakko.Core.Application.Unittests/                    # Unit tests only
dotnet test tests/MadWorldEU.Byakko.Controller.Api.IntegrationTests/ -- --coverage # With code coverage
```

The API integration tests spin up an in-process test server via `WebApplicationFactory` — no running instance or Docker is needed for them.