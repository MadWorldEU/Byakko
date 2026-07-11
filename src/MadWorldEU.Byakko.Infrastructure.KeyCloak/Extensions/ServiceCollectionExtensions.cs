using Keycloak.AuthServices.Sdk;
using MadWorldEU.Byakko.AuthenticationServers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MadWorldEU.Byakko.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKeyCloak(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddKeycloakAdminHttpClient(new KeycloakAdminClientOptions
        {
            AuthServerUrl = "http://localhost:8080/",
            Realm = "master",
            Resource = "admin-api",
        });
        
        services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();

        return services;
    }
}