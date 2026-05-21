# Kubernetes

This guide provides information on how to manage and deploy applications using Kubernetes, including best practices and common configurations.

## Development environment
## Development environment
### Activate Kubernetes in Docker Desktop
* Open Docker Desktop.
* Go to Settings > Kubernetes.
* Enable the checkbox: Enable Kubernetes.
* Wait for Kubernetes to start (you'll see a green light or similar status when ready).

### Install Required Tools
Make sure you have the following installed:
* [kubectl](https://kubernetes.io/docs/tasks/tools/) – Kubernetes command-line tool.
* [helm](https://helm.sh/docs/intro/install/) – Kubernetes package manager.

### Kubernetes Dashboard
Enable the Kubernetes Dashboard by installing [Headlamp](https://headlamp.dev/docs/latest/installation/desktop/):

#### Windows

Using **winget**:
```shell
winget install headlamp
```

Using **Chocolatey**:
```shell
choco install headlamp
```

Or download the `.exe` installer directly from the [latest release](https://github.com/kubernetes-sigs/headlamp/releases/latest).

#### macOS

Using **Homebrew** (recommended):
```shell
brew install --cask
```

Or download the `.dmg` file from the [latest release](https://github.com/kubernetes-sigs/headlamp/releases/latest).

If macOS blocks the app from running, open a terminal and run:
```shell
xattr -dr com.apple.quarantine /Applications/Headlamp.app
```
After this, running the app should work.

#### Open the Dashboard
Launch Headlamp and select your local Docker Desktop Kubernetes cluster. The dashboard gives you a visual overview of your cluster resources, workloads, and namespaces.

### Install Traefik
Install Traefik as the ingress controller:
```shell
helm repo add traefik https://traefik.github.io/charts
helm repo update
helm install traefik traefik/traefik -n traefik --create-namespace
helm upgrade traefik traefik/traefik -n traefik --set-json 'providers.kubernetesIngress.namespaces=["byakko-development"]'
```

### Setup TLS with mkcert
Install [mkcert](https://github.com/FiloSottile/,) and create locally-trusted certificates:
```shell
mkcert -install
mkcert byakko.dev "*.byakko.dev"
kubectl create secret tls umiko-tls \
  --cert=byakko.dev+1.pem \
  --key=byakko.dev+1-key.pem \
  -n byakko-development
```

### Configure Hosts File
Add the following entries to your hosts file so the local domains resolve to your machine:

**Windows**: `C:\Windows\System32\drivers\etc\hosts`
**macOS / Linux**: `/etc/hosts`

```
127.0.0.1       byakko.dev
127.0.0.1       www.byakko.dev
127.0.0.1       admin.byakko.dev
127.0.0.1       api.byakko.dev
127.0.0.1       database.byakko.dev
127.0.0.1       authentication.byakko.dev
```

### Deploy to Development
Navigate to the folder `deployments/helm/byakko` and execute the commands below.

#### Install
```shell
helm install -f Values.yaml -f Values.Development.yaml byakko .
```

#### Upgrade
```shell
helm upgrade -f Values.yaml -f Values.Development.yaml byakko .
```

#### Remove
```shell
helm uninstall -f byakko .
```