using MadWorldEU.Byakko.Audits;

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
        services.AddScoped<IAuditRepository, AuditRepository>();

        var migrationOptions = configuration.GetSection(MigrationOptions.SectionName).Get<MigrationOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section '{MigrationOptions.SectionName}'.");
        if (migrationOptions.AutoMigrate)
            services.AddHostedService<MigrationService>();

        return services;
    }
}