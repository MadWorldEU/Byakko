namespace MadWorldEU.Byakko.Configurations;

/// <summary>Registers the default CORS policy configured from <see cref="CorsSettings"/>.</summary>
internal static class CorsExtensions
{
    /// <summary>Adds CORS with origins from appsettings. Falls back to allowing any origin when <c>AllowedOrigins</c> is empty.</summary>
    internal static IServiceCollection AddDefaultCors(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(CorsSettings.Key).Get<CorsSettings>() ?? new CorsSettings();

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (settings.AllowedOrigins.Length > 0)
                {
                    policy.WithOrigins(settings.AllowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithExposedHeaders("Content-Disposition");
                }
                else
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithExposedHeaders("Content-Disposition");
                }
            });
        });

        return services;
    }
}