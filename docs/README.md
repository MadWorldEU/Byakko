# docs

This folder contains the project documentation for Byakko, including user guides, developer guides, architecture diagrams, and reference material.

## User Guide

| Document                                     | Description                      |
|----------------------------------------------|----------------------------------|
| [File Upload](user-guides/FileUpload.md)     | Upload and manage digital assets |

## Developer Guide

| Document                                         | Description                                                                 |
|--------------------------------------------------|-----------------------------------------------------------------------------|
| [Install Guide](developer-guides/InstallGuide.md)                 | How to set up and run Byakko locally for development                        |
| [Aspire](developer-guides/Aspire.md)                              | Aspire orchestration and run modes (Project, DockerFile, ContainerImage)    |
| [Authentication Server](developer-guides/AuthenticationServer.md) | Keycloak setup for local development and testing                            |
| [Database](developer-guides/Database.md)                          | EF Core / PostgreSQL setup, migrations, and connection string configuration |
| [DNS](developer-guides/DNS.md)                                    | DNS records required for production deployment                              |
| [Pipelines](developer-guides/Pipelines.md)                        | CI/CD pipelines for building, testing, Docker, and SonarCloud               |
| [DocFX](developer-guides/Docfx.md)                                | Building and serving the documentation site locally with DocFX              |
| [Setup Server](developer-guides/SetupServer.md)                   | Prepare an Ubuntu 25.04 server for production deployment                    |
| [Ubuntu Dev Environment](developer-guides/UbuntuDevEnvironment.md) | HTTPS certificates and Docker user setup on Ubuntu 25.04                   |
| [Kubernetes](developer-guides/Kubernetes.md)                      | Deploy and manage the platform on Kubernetes with Helm                      |
| [Rider Settings](developer-guides/RiderSettings.md)               | JetBrains Rider configuration for this project                              |

## Architecture

| Document                                         | Description                                                           |
|--------------------------------------------------|-----------------------------------------------------------------------|
| [System Context](diagrams/SystemContext.md)                        | High-level view of users and the system                               |
| [Containers](diagrams/Containers.md)                               | Internal containers and their interactions                            |
| [Structurizr](developer-guides/Structurizr.md)                    | Editing and exporting C4 architecture diagrams using Structurizr Lite |

## Reference

| Document                          | Description                                                     |
|-----------------------------------|-----------------------------------------------------------------|
| [Useful Links](developer-guides/UsefulLinks.md)    | Curated list of resources and references used in this project   |
| [Versions](developer-guides/Versions.md)           | Versioning strategy using Git tags and Semantic Versioning      |

