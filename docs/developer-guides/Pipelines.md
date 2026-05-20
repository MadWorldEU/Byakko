# Pipelines

This repository has three CI/CD pipelines:

- **Build and Test** — builds the solution and runs all tests on every push and pull request.
- **Build & Push Docker** — builds and pushes multi-arch Docker images for the API, Portal, and Admin to the GitHub Container Registry (GHCR) on every push to `main`.
- **SonarCloud** — runs static code analysis and reports code quality, security hotspots, and coverage to [SonarCloud](https://sonarcloud.io) on every push and pull request.

## Required Repository Secrets

Go to **Settings → Secrets and variables → Actions → New repository secret** and add:

| Secret          | Value                                                                   |
|-----------------|-------------------------------------------------------------------------|
| `GHCR_USERNAME` | Your GitHub username                                                    |
| `GHCR_TOKEN`    | A Personal Access Token with `write:packages` scope (see below)         |
| `SONAR_TOKEN`   | A SonarCloud token for the `MadWorldEU_Byakko` project (see below)      |

## Generating a GHCR_TOKEN

1. Go to **GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)**
2. Click **Generate new token (classic)**
3. Give it a descriptive name (e.g. `Byakko GHCR Push`)
4. Set an expiration date
5. Select the following scope:
   - `write:packages` — uploads packages to GitHub Package Registry (includes `read:packages`)
6. Click **Generate token**
7. Copy the token immediately — it won't be shown again
8. Paste it as the value of the `GHCR_TOKEN` secret in the repository settings

## Generating a SONAR_TOKEN

1. Go to [sonarcloud.io](https://sonarcloud.io) and open the `MadWorldEU_Byakko` project
2. Navigate to **Administration → Analysis Method**
3. Turn off **Automatic Analysis** — this is required when using GitHub Actions, as both cannot run simultaneously
4. Click **With GitHub Actions** to reveal the token
5. Copy the token shown on that page
6. Paste it as the value of the `SONAR_TOKEN` secret in the repository settings

## First-time: Linking Images to the Repository

After the pipeline runs for the first time, each image is created as a private package under the `MadWorldEU` organisation. Do the following once per image to connect it to this repository:

1. Go to **github.com/orgs/MadWorldEU/packages** (or your profile → Packages if using a personal account)
2. Click the package (e.g. `byakko-api`)
3. Click **Package settings** (bottom-right)
4. Under **Connect repository**, search for and select `MadWorldEU/Byakko`
5. Optionally change **Package visibility** to `Public` if the image should be publicly pullable
6. Repeat for `byakko-portal` and `byakko-admin`

Once linked, the packages appear on the repository's main page under **Packages**.

## Published Images

After a successful run the following images are available:

| Image  | URL                                        |
|--------|--------------------------------------------|
| API    | `ghcr.io/madworldeu/byakko-api:latest`     |
| Portal | `ghcr.io/madworldeu/byakko-portal:latest`  |
| Admin  | `ghcr.io/madworldeu/byakko-admin:latest`   |