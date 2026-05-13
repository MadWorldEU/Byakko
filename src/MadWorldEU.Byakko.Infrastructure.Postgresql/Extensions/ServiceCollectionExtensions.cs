namespace MadWorldEU.Byakko.Extensions;

/// <summary>Registers PostgreSQL infrastructure services with the dependency injection container.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgresql(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ByakkoContext>(options => 
            options.UseNpgsql(configuration.GetConnectionString("byakko-db"), 
                builder => builder.UseNodaTime()));

        services.AddScoped<IAssetRepository, AssetRepository>();

        var autoMigrate = configuration.GetValue<bool>(MigrationOptions.SectionName);
        if (autoMigrate)
            services.AddHostedService<MigrationService>();

        return services;
    }
}