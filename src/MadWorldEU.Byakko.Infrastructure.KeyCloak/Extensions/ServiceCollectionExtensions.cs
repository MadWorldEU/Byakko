using Keycloak.AuthServices.Common;
using Keycloak.AuthServices.Sdk;
using MadWorldEU.Byakko.Configurations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace MadWorldEU.Byakko.Extensions;

/// <summary>Registration extensions for the Keycloak infrastructure.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers the Keycloak admin HTTP client and authentication repository.</summary>
    public static IServiceCollection AddKeyCloak(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KeyCloakSettings>(configuration.GetSection(KeyCloakSettings.Key));

        services.AddTransient<KeyCloakTokenHandler>();

        services.AddKeycloakAdminHttpClient(new KeycloakAdminClientOptions()
            {
                AuthServerUrl = "http://localhost",
                Realm = "master"
            })
        .ConfigureHttpClient((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<KeyCloakSettings>>().Value;
            client.BaseAddress = new Uri(settings.AuthServerUrl.TrimEnd('/') + "/");
        })
        .AddHttpMessageHandler<KeyCloakTokenHandler>();
        
        services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
        services.Decorate<IAuthenticationRepository, CachedAuthenticationRepository>();

        return services;
    }
}