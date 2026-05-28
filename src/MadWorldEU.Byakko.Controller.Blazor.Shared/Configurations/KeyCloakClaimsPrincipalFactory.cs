using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;

namespace MadWorldEU.Byakko.Configurations;

public class KeyCloakClaimsPrincipalFactory : AccountClaimsPrincipalFactory<RemoteUserAccount>
{
    public KeyCloakClaimsPrincipalFactory(IAccessTokenProviderAccessor accessor) : base(accessor)
    {
    }

    public override async ValueTask<ClaimsPrincipal> CreateUserAsync(RemoteUserAccount account, RemoteAuthenticationUserOptions options)
    {
        var user = await base.CreateUserAsync(account, options);

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            return user;
        }

        if (user.Identity is ClaimsIdentity identity)
        {
            if (account.AdditionalProperties.TryGetValue("roles", out var rolesObj))
            {
                if (rolesObj is JsonElement rolesElement &&
                    rolesElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var role in rolesElement.EnumerateArray())
                    {
                        identity.AddClaim(
                            new Claim(ClaimTypes.Role, role.GetString()!));
                    }
                }
            }
        }
        
        return user;
    }
}
