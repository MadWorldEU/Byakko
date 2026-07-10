using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace MadWorldEU.Byakko.Handlers;

/// <summary>
/// Redirects to the logout page on 401 responses and when the OIDC refresh token is rejected
/// (e.g. Keycloak invalid_grant), ensuring the user session is cleared in both cases.
/// </summary>
internal sealed class UnauthorizedHandler(NavigationManager navigationManager) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                navigationManager.NavigateTo("/session-expired");
            }

            return response;
        }
        catch (AccessTokenNotAvailableException)
        {
            navigationManager.NavigateTo("/session-expired");
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }
    }
}