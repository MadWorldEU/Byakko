using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace MadWorldEU.Byakko.Configurations;

/// <summary>Registers JWT Bearer authentication and authorization services configured from <see cref="AuthenticationSettings"/>.</summary>
internal static class AuthenticationExtensions
{
    /// <summary>Adds JWT Bearer authentication backed by Keycloak and authorization to the service container.</summary>
    internal static void AddDefaultAuthentication(this WebApplicationBuilder builder)
    {
        var authenticationSettings = builder.Configuration
            .GetRequiredSection(AuthenticationSettings.Key)
            .Get<AuthenticationSettings>()
            ?? throw new InvalidOperationException($"Missing configuration section '{AuthenticationSettings.Key}'.");

        builder.Services.AddSingleton(Options.Create(authenticationSettings));

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authenticationSettings.Authority; // Keycloak realm URL
                options.Audience = authenticationSettings.Audience; // Match the "Client ID" of your Keycloak client
                options.RequireHttpsMetadata = false; // Only for dev/local

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = authenticationSettings.ValidateUser,
                    ValidateAudience = authenticationSettings.ValidateUser,
                    ValidateLifetime = authenticationSettings.ValidateUser,
                    ValidateIssuerSigningKey = authenticationSettings.ValidateUser,
                    NameClaimType = "preferred_username",
                    RoleClaimType = "roles"
                };

                if (!authenticationSettings.ValidateUser)
                {
                    options.TokenValidationParameters.SignatureValidator = (token, _) =>
                    {
                        // Just return the token without validating signature
                        var handler = new JsonWebTokenHandler();
                        return handler.ReadJsonWebToken(token);
                    };
                }

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger(nameof(AuthenticationExtensions));
                        logger.LogInformation(context.Exception, "Authentication failed for: {IdentityName}", context.Principal?.Identity?.Name ?? "Unknown");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger(nameof(AuthenticationExtensions));
                        logger.LogInformation("Token validated for: {IdentityName}",
                            context.Principal?.Identity?.Name ?? "Unknown");
                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddAuthorization();
    }
}