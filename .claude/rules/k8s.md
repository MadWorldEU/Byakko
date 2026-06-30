# Helm / Kubernetes

Chart in `deployments/helm/byakko/`. See `docs/developer-guides/Kubernetes.md`.

Subdomains: `byakko.dev`/`www.`/`fileshare.` → Portal; `api.` → API; `admin.` → Admin; `status.` → Status; `database.` → pgAdmin; `authentication.` → Keycloak; `grafana.` → Grafana; `kubernetes.` → Headlamp (`headlamp.enabled`).

All image tags via top-level `appVersion` in `values.yaml`. Storage config (`Storage:Mode`, `BucketName`, `OvhCloud.*`) shared by `api.yaml` and `status.yaml`.

**Headlamp:** `ExternalName` Service → `my-headlamp.kube-system.svc.cluster.local`. Separate `byakko-ingress-headlamp` (no `security-headers` middleware — COOP/COEP break OIDC redirect). Requires `headlamp-client` in Keycloak + microk8s kube-apiserver OIDC args. See Kubernetes.md Step 6.

**Grafana Keycloak auth:** Generic OAuth in `grafana.yaml`. Role mapping: `contains(roles, 'Administrator') && 'Admin' || ''`. **Critical:** realm roles mapper must have **Add to ID token** enabled — Grafana falls back to ID token only. Client secret: `GRAFANA_KEYCLOAK_CLIENT_SECRET`.

**pgAdmin Keycloak auth:** `config_local.py` ConfigMap. `AUTHENTICATION_SOURCES = ['oauth2']`. Restricted via `administrator-only-browser` Keycloak flow. Client secret: `PGADMIN_KEYCLOAK_CLIENT_SECRET`.

**Grafana dashboards:** JSON files in `grafana/dashboards/` auto-loaded via `.Files.Glob`. Datasource names must match exactly (`"Loki"`, `"Prometheus"`, `"Tempo"`). Stream matchers: `.+` not `.*`.

**Observability retention (60 days):** Loki: `retention_period: 1440h` + `retention_enabled: true`. Prometheus: `--storage.tsdb.retention.time=60d`. Tempo: `block_retention: 1440h`.

TLS local: `mkcert byakko.dev "*.byakko.dev"`. TLS production: `clusterIssuer.enabled = true` + cert-manager HTTP-01.