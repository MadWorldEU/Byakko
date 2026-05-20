---
_layout: landing
---

# Byakko

A file sharing platform for uploading, managing, and retrieving digital assets. Built with Clean Architecture on .NET 10, with Blazor WebAssembly frontends and a PostgreSQL + MinIO backend.

## User Guide

| Guide                                          | Description                          |
|------------------------------------------------|--------------------------------------|
| [File Upload](user-guides/FileUpload.md)       | Upload and manage digital assets     |

## Developer Guide

| Guide                                              | Description                                                   |
|----------------------------------------------------|---------------------------------------------------------------|
| [Install Guide](developer-guides/InstallGuide.md)                   | Set up and run the platform locally                           |
| [Aspire](developer-guides/Aspire.md)                                | Aspire orchestration and run modes (Project, DockerFile, ContainerImage) |
| [Authentication Server](developer-guides/AuthenticationServer.md)   | Configure Keycloak for JWT authentication                     |
| [Database](developer-guides/Database.md)                            | Run and manage PostgreSQL migrations                          |
| [Pipelines](developer-guides/Pipelines.md)                          | CI/CD pipelines for building, testing, Docker, and SonarCloud |
| [DocFX](developer-guides/Docfx.md)                                  | Build and deploy the documentation site                       |
| [Rider Settings](developer-guides/RiderSettings.md)                 | Recommended JetBrains Rider configuration                     |

## Architecture

| Guide                                              | Description                                       |
|----------------------------------------------------|---------------------------------------------------|
| [System Context](diagrams/SystemContext.md)        | High-level view of users and the system           |
| [Containers](diagrams/Containers.md)               | Internal containers and their interactions        |
| [Structurizr](developer-guides/Structurizr.md)                      | Editing and exporting C4 diagrams locally         |

## Reference

| Guide                              | Description                          |
|------------------------------------|--------------------------------------|
| [Useful Links](developer-guides/UsefulLinks.md)     | External links and resources         |
| [Versions](developer-guides/Versions.md)            | Changelog and version history        |