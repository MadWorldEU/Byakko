# Authentication Server
This folder sets up a local authentication server using **Keycloak** for development and testing purposes.

## Keycloak
### Getting Started at developer environment
Start the development environment using the Aspire Dashboard:

Once running, access Keycloak Admin UI using the dashboard.
Login with:
* Username: admin
* Password: keycloak-MySecretP@sswOrd

To configure a web client and an API client, follow the official guide:
[Keycloak - Getting Started with Docker](https://www.keycloak.org/getting-started/getting-started-docker)

### Getting Started at production environment

In production, Keycloak is deployed as a Kubernetes `StatefulSet` via the Helm chart. It is exposed at `authentication.<domain>` through the Traefik ingress with TLS.

1. Set the admin credentials in `values.production.yaml` under `keycloak.username` and `keycloak.password`.
2. Deploy the chart — Keycloak will start and create a fresh realm on first boot.
3. Access the Admin UI at `https://authentication.<domain>` and log in with the credentials above.
4. Follow the same configuration steps as the developer environment: create the clients, add the audience scope, configure the roles claim, and create the `Administrator` and `User` roles.

> 💡 The realm name and client IDs are controlled by `keycloak.realm`, `admin.oidc.clientId`, and `portal.oidc.clientId` in `values.yaml`. Make sure the values in Keycloak match these exactly.

### Add Audience Attribute (for JWT validation)

To include the correct `aud` (audience) claim in your tokens:

1. Go to **Client Scopes** in the Keycloak Admin UI.
2. Click **Create** to add a new client scope (e.g., `audience-api`).
3. Navigate to the **Mappers** tab of the client scope.
    - Click **Create**.
    - Set **Mapper Type** to `Audience`.
    - Set **Name** to `audience-api-mapper`.
    - Set **Included Client Audience** to your target client ID (e.g., `api-client`).
    - Enable **Add to access token** and **Add to ID token**.
    - Click **Save**.
4. Go to **Clients**, select the client that should include this audience.
5. Open the **Client Scopes** tab.
6. Add your new client scope as either **Default** or **Optional**.

> 💡 This ensures the `aud` field in the JWT token includes the specified client audience (e.g., `api-client`), which is commonly required for API-side JWT validation.

### Add Roles

Byakko requires two realm roles: `Administrator` and `User`. The token claim name must be `roles` so the API and Blazor apps can read them.

#### Create the roles

1. Go to **Realm roles** in the Keycloak Admin UI.
2. Click **Create role**.
3. Set **Role name** to `Administrator` and click **Save**.
4. Repeat to create the `User` role.

#### Set the Token Claim Name to `roles`

By default Keycloak maps realm roles under `realm_access.roles`. Change it to a flat `roles` claim:

1. Go to **Client Scopes** and open **roles**.
2. Open the **Mappers** tab and click **realm roles**.
3. Set **Token Claim Name** to `roles`.
4. Enable **Add to access token**, **Add to ID token**, and **Add to userinfo**.
5. Click **Save**.

#### Assign a role to an account (optional)

1. Go to **Users** and open the account.
2. Open the **Role mapping** tab.
3. Click **Assign role**, select `Administrator` or `User`, and confirm.

### Add Grafana Client

Grafana authenticates against Keycloak using a confidential OAuth2 client named `grafana-client`. Only users with the `Administrator` role can sign in — non-admins are rejected by the strict role attribute check.

1. Go to **Clients** in the Keycloak Admin UI and click **Create client**.
2. Set **Client ID** to `grafana-client` and click **Next**.
3. Enable **Client authentication** (this makes it a confidential client) and click **Next**.
4. Set **Valid redirect URIs** to `https://grafana.<domain>/login/generic_oauth`.
5. Click **Save**.
6. Open the **Credentials** tab and copy the **Client secret** — use it as the `GRAFANA_KEYCLOAK_CLIENT_SECRET` environment secret in the deployment pipeline.
7. Go to **Clients** → **grafana-client** → **Advanced** tab → **Authentication flow overrides** → set **Browser Flow** to `administrator-only-browser` → **Save**.

After this, non-Administrator users receive "You don't have access to this application" from Keycloak before reaching Grafana.

> 💡 The `administrator-only-browser` flow must be created first — see **Restrict Headlamp login to Administrators only** under [Step 6 — Connect Headlamp with Keycloak](Kubernetes.md#step-6--connect-headlamp-with-keycloak-optional) in the Kubernetes guide for the step-by-step flow setup. The same flow is reused here.

> 💡 The `roles` claim must already be mapped to the flat `roles` token claim (see [Set the Token Claim Name to `roles`](#set-the-token-claim-name-to-roles)) — Grafana's role attribute path `contains(roles[*], 'Administrator') && 'Admin' || ''` depends on it.

### Add pgAdmin Client

pgAdmin authenticates against Keycloak using a confidential OAuth2 client named `pgadmin-client`. Only users with the `Administrator` role can sign in — non-admins are rejected at the Keycloak level before reaching pgAdmin.

1. Go to **Clients** in the Keycloak Admin UI and click **Create client**.
2. Set **Client ID** to `pgadmin-client` and click **Next**.
3. Enable **Client authentication** (this makes it a confidential client) and click **Next**.
4. Set **Valid redirect URIs** to `https://database.<domain>/oauth2/authorize`.
5. Click **Save**.
6. Open the **Credentials** tab and copy the **Client secret** — use it as the `PGADMIN_KEYCLOAK_CLIENT_SECRET` environment secret in the deployment pipeline.
7. Go to **Clients** → **pgadmin-client** → **Advanced** tab → **Authentication flow overrides** → set **Browser Flow** to `administrator-only-browser` → **Save**.

After this, non-Administrator users receive "You don't have access to this application" from Keycloak before reaching pgAdmin.

> 💡 The `administrator-only-browser` flow must be created first — see **Restrict Headlamp login to Administrators only** under [Step 6 — Connect Headlamp with Keycloak](Kubernetes.md#step-6--connect-headlamp-with-keycloak-optional) in the Kubernetes guide for the step-by-step flow setup. The same flow is reused here.

### Enable Login Settings

1. Go to **Realm settings** in the Keycloak Admin UI.
2. Open the **Login** tab.
3. Enable the following options:
   - **User registration** — allows new users to self-register
   - **Forgot password** — shows a password reset link on the login screen
   - **Remember me** — lets users stay logged in across browser sessions
   - **Verify email** — requires users to verify their email address after registration
4. Click **Save**.

> 💡 In the developer environment, **User registration** is enabled automatically via `MadWorld-realm.json`. In production, enable it manually after first deploy.

### Configure Session Timeouts

Byakko uses a 1-day session for normal logins and a 7-day session when users tick **Remember me**.

1. Go to **Realm settings** in the Keycloak Admin UI.
2. Open the **Sessions** tab.
3. Set the following values:

| Setting | Value |
|---|---|
| SSO Session Idle | 1 day |
| SSO Session Max | 1 day |
| SSO Session Idle Remember Me | 7 days |
| SSO Session Max Remember Me | 7 days |

4. Click **Save**.

> 💡 In the developer environment these values are set automatically via `MadWorld-realm.json`. In production, apply them manually after the first deploy.

### Terms and Conditions

Byakko shows a Terms and Conditions acceptance screen (linking to `/user-agreement`) before users can complete their first login. This is implemented via a Keycloak Required Action and a custom `byakko` login theme.

#### How it works

- The `byakko` login theme overrides only the `termsText` message key. All other UI inherits from `keycloak.v2`.
- The Required Action `TERMS_AND_CONDITIONS` intercepts the login flow and presents the acceptance screen.
- `defaultAction: true` means every user (new and existing) must accept once.

#### Developer environment (Aspire)

This is configured automatically:

- `MadWorld-realm.json` has `"loginTheme": "byakko"` and `TERMS_AND_CONDITIONS` enabled with `defaultAction: true`.
- The theme files are bind-mounted from `src/MadWorldEU.Byakko.Aspire/Configurations/KeyCloak/themes/` into the Keycloak container.

To change the terms link, edit `themes/byakko/login/messages/messages_en.properties` and restart the Keycloak container.

#### Production environment (Helm)

The theme files are injected via the `keycloak-theme-byakko` ConfigMap in `templates/keycloak.yaml`. The `termsText` URL is derived from `{{ .Values.ingress.domain }}` and resolves to `https://byakko.dev/user-agreement`.

To enable the Required Action in a fresh production realm:

1. Go to **Authentication** → **Required Actions** in the Keycloak Admin UI.
2. Find **Terms and Conditions** and toggle **Enabled** on.
3. Toggle **Default Action** on so existing users are prompted on next login.

#### Enable localization in production

The Helm chart injects translated `termsText` for English, Dutch, and Japanese, but Keycloak will only use them once localization is enabled at the realm level:

1. Go to **Realm Settings** → **Localization** tab.
2. Toggle **Internationalization** on.
3. Add `en`, `nl`, and `ja` under **Supported Locales**.
4. Set **Default Locale** to `en`.
5. Click **Save**.

> 💡 In the developer environment this is configured automatically via `MadWorld-realm.json`. In production it must be done manually after first deploy.

### Configure Email

Keycloak uses an SMTP server to send verification, password reset, and other system emails. To configure it:

1. Go to **Realm settings** in the Keycloak Admin UI.
2. Open the **Email** tab.
3. Fill in the following fields:
   - **From** — The sender email address (e.g. `noreply@example.com`)
   - **From display name** — The human-readable sender name shown in email clients (e.g. `Byakko`)
   - **Host** — The SMTP server hostname (e.g. `smtp.example.com`)
   - **Port** — The SMTP port (e.g. `587` for StartTLS, `465` for SSL)
   - **Enable StartTLS** — Toggle on when using port 587 to upgrade the connection to TLS
   - **Username** — The SMTP account username used to authenticate
   - **Password** — The SMTP account password used to authenticate
4. Click **Save**, then use the **Test connection** button to verify Keycloak can reach the mail server.

### Test Login with Keycloak Using the Official Test App
You can verify that your Keycloak server is correctly issuing tokens (including the `aud` claim) by using Keycloak's official test app:

https://www.keycloak.org/app/