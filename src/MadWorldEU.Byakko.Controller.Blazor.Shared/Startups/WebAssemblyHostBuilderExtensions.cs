using System.Security.Claims;
using MadWorldEU.Byakko.Application;
using MadWorldEU.Byakko.Configurations;
using MadWorldEU.Byakko.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MadWorldEU.Byakko.Startups;

public static class WebAssemblyHostBuilderExtensions
{
    public static WebAssemblyHostBuilder AddByakkoApiHttpClients(this WebAssemblyHostBuilder builder)
    {
        var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;
        builder.Services.AddHttpClient(HttpClients.ApiAnonymous, client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        });

        builder.Services.AddHttpClient(HttpClients.ApiAuthorized, client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.Timeout = Timeout.InfiniteTimeSpan;
        }).AddHttpMessageHandler(sp =>
        {
            var handler = sp.GetRequiredService<AuthorizationMessageHandler>()
                .ConfigureHandler(authorizedUrls: [apiBaseUrl]);
            return handler;
        });

        return builder;
    }

    public static WebAssemblyHostBuilder AddByakkoApplication(this WebAssemblyHostBuilder builder)
    {
        builder.Services.AddScoped<GeneratePasswordUseCase>();
        return builder;
    }

    public static WebAssemblyHostBuilder AddByakkoServices(this WebAssemblyHostBuilder builder)
    {
        builder.Services.Configure<AssetSettings>(builder.Configuration.GetSection(AssetSettings.Key));
        builder.Services.Configure<OidcSettings>(builder.Configuration.GetSection(OidcSettings.Key));
        builder.Services.AddScoped<IAuditService, AuditService>();
        builder.Services.AddScoped<IAssetService, AssetService>();
        builder.Services.AddScoped<IStorageService, StorageService>();

        return builder;
    }

    public static WebAssemblyHostBuilder AddByakkoAuthentication(this WebAssemblyHostBuilder builder)
    {
        builder.Services.AddOidcAuthentication(options =>
        {
            builder.Configuration.Bind("Oidc", options.ProviderOptions);
            options.UserOptions.RoleClaim = ClaimTypes.Role;
        }).AddAccountClaimsPrincipalFactory<KeyCloakClaimsPrincipalFactory>();
        
        builder.Services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(AuthorizationPolicies.Administrator, policy =>
                policy.RequireRole(AuthorizationRoles.Administrator));
    
            options.AddPolicy(AuthorizationPolicies.User, policy =>
                policy.RequireRole(AuthorizationRoles.User));
        });

        return builder;
    }
}