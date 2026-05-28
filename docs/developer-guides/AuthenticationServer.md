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

### Test Login with Keycloak Using the Official Test App
You can verify that your Keycloak server is correctly issuing tokens (including the `aud` claim) by using Keycloak's official test app:

https://www.keycloak.org/app/