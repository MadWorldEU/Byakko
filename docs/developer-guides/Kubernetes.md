# Kubernetes

This guide provides information on how to manage and deploy applications using Kubernetes, including best practices and common configurations.

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
helm upgrade traefik traefik/traefik -n traefik \
  --set-json 'providers.kubernetesIngress.namespaces=["byakko-development"]' \
  --set ports.web.transport.respondingTimeouts.readTimeout=0 \
  --set ports.websecure.transport.respondingTimeouts.readTimeout=0
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
127.0.0.1       status.byakko.dev
127.0.0.1       api.byakko.dev
127.0.0.1       database.byakko.dev
127.0.0.1       authentication.byakko.dev
127.0.0.1       grafana.byakko.dev
127.0.0.1       kubernetes.byakko.dev
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

## Production environment

Before starting, make sure your server is set up according to the [server setup guide](SetupServer.md).

### Install on production

#### Step 1: Install MicroK8s

```shell
sudo snap install microk8s --classic
sudo microk8s status --wait-ready
```

#### Step 2: Enable services

Required:

```shell
sudo microk8s enable dns
sudo microk8s enable helm
sudo microk8s enable cert-manager
sudo microk8s enable hostpath-storage
```

Optional:

```shell
sudo microk8s enable metrics-server
sudo microk8s enable prometheus
```

#### Step 3: Install Traefik

Install Traefik as the ingress controller:

```shell
sudo microk8s helm repo add traefik https://traefik.github.io/charts
sudo microk8s helm repo update
sudo microk8s helm install traefik traefik/traefik -n traefik --create-namespace
sudo microk8s helm upgrade traefik traefik/traefik -n traefik \
  --set ports.web.hostPort=80 \
  --set ports.websecure.hostPort=443 \
  --set "additionalArguments={--entrypoints.web.http.redirections.entryPoint.to=:443,--entrypoints.web.http.redirections.entryPoint.scheme=https}" \
  --set deployment.strategy.type=Recreate \
  --set ports.web.transport.respondingTimeouts.readTimeout=0 \
  --set ports.websecure.transport.respondingTimeouts.readTimeout=0
```

If the new Traefik pod is stuck in `Pending` after an upgrade, the old pod may still be holding ports 80/443. Delete it manually:

```shell
sudo microk8s kubectl delete pod <old-traefik-pod-name> -n traefik
```

### Configure DNS

Before deploying, make sure DNS A records are configured for your domain. See the [DNS configuration guide](DNS.md) for details.

### Usage on production

#### Step 1 — Download source code

```shell
git clone https://github.com/MadWorldEU/Byakko
```

#### Step 2 — Install or upgrade cluster

Navigate to the folder `deployments/helm/byakko` and execute one of the commands below.

Install:

```shell
sudo microk8s helm install -f Values.yaml -f Values.Production.yaml byakko .
```

Upgrade:

```shell
sudo microk8s helm upgrade -f Values.yaml -f Values.Production.yaml byakko .
```

#### Step 3 — Install Headlamp

Install [Headlamp](https://headlamp.dev/) as the Kubernetes dashboard:

```shell
sudo microk8s helm repo add headlamp https://kubernetes-sigs.github.io/headlamp/
sudo microk8s helm repo update
sudo microk8s helm install my-headlamp headlamp/headlamp --namespace kube-system
```

#### Step 4 — Create Headlamp token

Create a token for logging in to Headlamp:

```shell
sudo microk8s kubectl create token my-headlamp --namespace kube-system
```

#### Step 5 — Access Headlamp

**Option A — Via the byakko ingress (recommended for production)**

After deploying the Helm chart with `headlamp.enabled: true`, access the dashboard at:

```
https://kubernetes.<domain>
```

Use the token from Step 4 to log in.

**Option B — Via port forwarding (local / without ingress)**

Forward the Headlamp port to access the dashboard:

```shell
export POD_NAME=$(sudo microk8s kubectl get pods --namespace kube-system -l "app.kubernetes.io/name=headlamp,app.kubernetes.io/instance=my-headlamp" -o jsonpath="{.items[0].metadata.name}")
export CONTAINER_PORT=$(sudo microk8s kubectl get pod --namespace kube-system $POD_NAME -o jsonpath="{.spec.containers[0].ports[0].containerPort}")
sudo microk8s kubectl --namespace kube-system port-forward --address 0.0.0.0 $POD_NAME 10443:$CONTAINER_PORT
```

Then open `http://localhost:10443` and use the token from Step 4 to log in.

#### Step 6 — Connect Headlamp with Keycloak (optional)

By default Headlamp uses token-based login. To use Keycloak instead, follow these steps.

**Create a Keycloak client**

In the `MadWorld` realm at `https://authentication.<domain>`, create a new client with these settings:

| Setting                  | Value                                   |
|--------------------------|-----------------------------------------|
| Client ID                | `headlamp-client`                       |
| Client authentication    | On (confidential)                       |
| Valid redirect URIs      | `https://kubernetes.<domain>/*`         |
| Web origins              | `https://kubernetes.<domain>`           |

Then copy the client secret from the **Credentials** tab.

**Upgrade Headlamp with OIDC configuration**

```shell
sudo microk8s helm upgrade my-headlamp headlamp/headlamp --namespace kube-system \
  --set config.oidc.issuerURL="https://authentication.<domain>/realms/MadWorld" \
  --set config.oidc.clientID="headlamp-client" \
  --set config.oidc.clientSecret="<<SECRET>>" \
  --set config.oidc.scopes="profile email" 
```

After this, Headlamp redirects unauthenticated users to Keycloak automatically.

**Grant Kubernetes permissions to Keycloak users**

Keycloak controls authentication; Kubernetes RBAC controls what users can do. Create a `ClusterRoleBinding` to grant a Keycloak user read-only access to the cluster:

```shell
sudo microk8s kubectl create clusterrolebinding headlamp-view \
  --clusterrole=cluster-admin \
  --user=madworld@oscarveldman.eu
```

Replace `view` with `cluster-admin` to grant full access, or create a custom `ClusterRole` for finer-grained control.

### Before a server shutdown

Gracefully drain the cluster before shutting down to avoid data corruption and incomplete requests.

#### Step 1 — Drain all workloads

```shell
sudo microk8s kubectl drain <node-name> --ignore-daemonsets --delete-emptydir-data
```

Replace `<node-name>` with the output of:

```shell
sudo microk8s kubectl get nodes
```

#### Step 2 — Stop MicroK8s

```shell
sudo microk8s stop
```

#### Step 3 — Shut down the server

```shell
sudo shutdown -h now
```

### After a server reboot

MicroK8s does not start automatically after a reboot. Follow the steps below to bring the cluster back up.

#### Step 1 — Start MicroK8s

```shell
sudo microk8s start
```

#### Step 2 — Uncordon the node

If the node was drained before shutdown, mark it schedulable again:

```shell
sudo microk8s kubectl uncordon <node-name>
```

#### Step 3 — Wait for MicroK8s to be ready

```shell
sudo microk8s status --wait-ready
```

#### Step 4 — Verify all pods are running

```shell
sudo microk8s kubectl get pods -A
```

All pods should reach `Running` or `Completed` status within a few minutes. If any pod is stuck in `Pending` or `CrashLoopBackOff`, inspect it with:

```shell
sudo microk8s kubectl describe pod <pod-name> -n <namespace>
sudo microk8s kubectl logs <pod-name> -n <namespace>
```

### Reference

- [MicroK8s install guide](https://microk8s.io/)