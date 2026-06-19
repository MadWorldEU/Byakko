using System.Diagnostics.CodeAnalysis;
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

        if (!IsAuthenticated(user, out var identity))
        {
            return user;
        }
        
        var userRoles = GetUserRoles(account);
        foreach (var role in userRoles)
        {
            identity.AddClaim(
                new Claim(ClaimTypes.Role, role));
        }

        return user;
    }

    private static bool IsAuthenticated(ClaimsPrincipal user, [NotNullWhen(true)]out ClaimsIdentity? identity)
    {
        identity = null;
        
        if (!(user.Identity?.IsAuthenticated ?? true) || user.Identity is not ClaimsIdentity claimsIdentity)
        {
            return false;
        }
        
        identity = claimsIdentity;
        return true;

    }

    private static List<string> GetUserRoles(RemoteUserAccount account)
    {
        if (account.AdditionalProperties.TryGetValue("roles", out var rolesObj) && 
            rolesObj is JsonElement { ValueKind: JsonValueKind.Array } rolesElement)
        {
            return rolesElement.EnumerateArray().Select(e => e.ToString()).ToList();
        }

        return [];
    }
}
