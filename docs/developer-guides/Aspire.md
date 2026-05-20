# Aspire

The `MadWorldEU.Byakko.Aspire` project is the .NET Aspire AppHost that orchestrates all services locally. It starts and wires together PostgreSQL, LocalStack (S3), Keycloak, and the three application services (API, Admin, Portal), injecting connection strings and waiting for health checks before dependent services start.

## Run Modes

The AppHost supports three run modes, controlled by the `RunMode` key in `appsettings.json`:

```json
"RunMode": "Project"
```

| Mode | Value | Description |
|---|---|---|
| Project reference | `Project` | Runs services directly from source via project references. The default for local development. |
| Local Dockerfile | `DockerFile` | Builds and runs container images from the local Dockerfiles. Useful for testing containerisation before pushing. |
| Published image | `ContainerImage` | Pulls and runs the pre-built images from GHCR. Use this to test the exact images that CI has published. |

### Project (default)

No additional setup required. Services are compiled and run by Aspire directly from source, with hot reload and debugger support.

### DockerFile

Requires Docker to be running. Aspire builds each image from its Dockerfile at the repository root before starting the container. This is slower than `Project` mode but validates the Dockerfile and container entrypoint locally without a registry push.

Each Dockerfile must be built from the repository root — Aspire handles this automatically using `../../` as the build context.

### ContainerImage

Requires Docker to be running. Aspire pulls the images defined in `Factories/DockerImages.cs`. The tag is pinned in that file — update it to target a specific release or change it to `latest` to always pull the most recent build from `main`.

Images are published to GHCR by the CI pipeline on every push to `main` for both `linux/amd64` and `linux/arm64`.