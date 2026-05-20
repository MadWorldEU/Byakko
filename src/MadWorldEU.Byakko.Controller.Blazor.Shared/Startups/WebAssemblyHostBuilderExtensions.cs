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
        }).AddHttpMessageHandler(sp =>
        {
            var handler = sp.GetRequiredService<AuthorizationMessageHandler>()
                .ConfigureHandler(authorizedUrls: [apiBaseUrl]);
            return handler;
        });

        return builder;
    }

    public static WebAssemblyHostBuilder AddByakkoAuthentication(this WebAssemblyHostBuilder builder)
    {
        builder.Services.AddOidcAuthentication(options =>
        {
            builder.Configuration.Bind("Oidc", options.ProviderOptions);
        });

        return builder;
    }
}