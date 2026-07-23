using Keycloak.AuthServices.Common;
using Keycloak.AuthServices.Sdk;
using MadWorldEU.Byakko.AuthenticationServers;
using MadWorldEU.Byakko.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MadWorldEU.Byakko.Extensions;

/// <summary>Registration extensions for the Keycloak infrastructure.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers the Keycloak admin HTTP client and authentication repository.</summary>
    public static IServiceCollection AddKeyCloak(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(KeyCloakSettings.Key).Get<KeyCloakSettings>()
            ?? throw new InvalidOperationException("KeyCloak configuration section is missing.");

        services.Configure<KeyCloakSettings>(configuration.GetSection(KeyCloakSettings.Key));

        services.AddTransient<KeyCloakTokenHandler>();

        services.AddKeycloakAdminHttpClient(new KeycloakAdminClientOptions
        {
            AuthServerUrl = settings.AuthServerUrl,
            Realm = "master",
            Resource = settings.Resource,
            SslRequired = settings.SslRequired ? "Internal" : "none",
            VerifyTokenAudience = true,
            Credentials = new KeycloakClientInstallationCredentials
            {
                Secret = settings.AdminClientSecret
            },
        }).AddHttpMessageHandler<KeyCloakTokenHandler>();

        services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();

        return services;
    }
}