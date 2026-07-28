using System.Text;
using System.Text.Json;

namespace MadWorldEU.Byakko.Common;

/// <summary>Thin wrapper around the Keycloak Admin REST API for use in integration tests.</summary>
internal sealed class KeycloakAdminTestClient(string baseUrl, string adminClientSecret)
{
    private readonly string _baseUrl = baseUrl.TrimEnd('/');

    /// <summary>Gets a token using the master-realm admin user (admin-cli password grant). Use for master-realm setup operations only.</summary>
    private async Task<string> GetAdminTokenAsync()
    {
        using var client = new HttpClient();
        var response = await client.PostAsync(
            $"{_baseUrl}/realms/master/protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = "admin-cli",
                ["username"] = "admin",
                ["password"] = "admin"
            }));
        response.EnsureSuccessStatusCode();
        var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
        return json.GetProperty("access_token").GetString()!;
    }

    /// <summary>Gets a token using the madworld-admin-api service account (client credentials). Use for managed-realm operations.</summary>
    private async Task<string> GetServiceAccountTokenAsync()
    {
        using var client = new HttpClient();
        var response = await client.PostAsync(
            $"{_baseUrl}/realms/master/protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = "madworld-admin-api",
                ["client_secret"] = adminClientSecret
            }));
        response.EnsureSuccessStatusCode();
        var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
        return json.GetProperty("access_token").GetString()!;
    }

    private async Task<HttpClient> CreateAdminClientAsync()
    {
        var token = await GetAdminTokenAsync();
        var client = new HttpClient { BaseAddress = new Uri(_baseUrl) };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<HttpClient> CreateServiceAccountClientAsync()
    {
        var token = await GetServiceAccountTokenAsync();
        var client = new HttpClient { BaseAddress = new Uri(_baseUrl) };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>Creates a realm with the given <paramref name="realmName"/> in Keycloak.</summary>
    public async Task CreateRealmAsync(string realmName)
    {
        using var client = await CreateAdminClientAsync();
        var body = JsonSerializer.Serialize(new { realm = realmName, enabled = true });
        (await client.PostAsync(
            "/admin/realms",
            new StringContent(body, Encoding.UTF8, "application/json"))).EnsureSuccessStatusCode();
    }

    /// <summary>Creates a confidential service-account client in master realm and grants it manage-users on <paramref name="managedRealm"/>.</summary>
    public async Task CreateAdminServiceAccountAsync(string clientSecret, string managedRealm)
    {
        using var client = await CreateAdminClientAsync();

        var createBody = JsonSerializer.Serialize(new
        {
            clientId = "madworld-admin-api",
            secret = clientSecret,
            serviceAccountsEnabled = true,
            publicClient = false,
            directAccessGrantsEnabled = false
        });
        (await client.PostAsync("/admin/realms/master/clients",
            new StringContent(createBody, Encoding.UTF8, "application/json"))).EnsureSuccessStatusCode();

        var clientsJson = await (await client.GetAsync("/admin/realms/master/clients?clientId=madworld-admin-api"))
            .Content.ReadAsStringAsync();
        var clients = JsonSerializer.Deserialize<JsonElement[]>(clientsJson)!;
        var clientInternalId = clients[0].GetProperty("id").GetString()!;

        var serviceAccountJson = await (await client.GetAsync($"/admin/realms/master/clients/{clientInternalId}/service-account-user"))
            .Content.ReadAsStringAsync();
        var serviceAccountUserId = JsonSerializer.Deserialize<JsonElement>(serviceAccountJson).GetProperty("id").GetString()!;

        var realmClientJson = await (await client.GetAsync($"/admin/realms/master/clients?clientId={managedRealm}-realm"))
            .Content.ReadAsStringAsync();
        var realmClients = JsonSerializer.Deserialize<JsonElement[]>(realmClientJson)!;
        var realmClientId = realmClients[0].GetProperty("id").GetString()!;

        var rolesJson = await (await client.GetAsync($"/admin/realms/master/clients/{realmClientId}/roles"))
            .Content.ReadAsStringAsync();
        var roles = JsonSerializer.Deserialize<JsonElement[]>(rolesJson)!;

        var requiredRoles = new[] { "manage-users", "query-users", "view-users" };
        var roleBody = JsonSerializer.Serialize(
            roles
                .Where(r => requiredRoles.Contains(r.GetProperty("name").GetString()))
                .Select(r => new { id = r.GetProperty("id").GetString(), name = r.GetProperty("name").GetString() })
                .ToArray());
        (await client.PostAsync(
            $"/admin/realms/master/users/{serviceAccountUserId}/role-mappings/clients/{realmClientId}",
            new StringContent(roleBody, Encoding.UTF8, "application/json"))).EnsureSuccessStatusCode();
    }

    /// <summary>Creates a user in <paramref name="realm"/> with the given <paramref name="username"/> and returns the Keycloak-assigned user ID.</summary>
    public async Task<string> CreateUserAsync(string realm, string username)
    {
        using var client = await CreateServiceAccountClientAsync();
        var body = JsonSerializer.Serialize(new { username, enabled = true });
        var response = await client.PostAsync(
            $"/admin/realms/{realm}/users",
            new StringContent(body, Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();

        var location = response.Headers.Location!.ToString();
        return location.Split('/').Last();
    }

    /// <summary>Returns true if a user with the given <paramref name="userId"/> exists in <paramref name="realm"/>.</summary>
    public async Task<bool> UserExistsAsync(string realm, string userId)
    {
        using var client = await CreateServiceAccountClientAsync();
        var response = await client.GetAsync($"/admin/realms/{realm}/users/{userId}");
        return response.IsSuccessStatusCode;
    }
}