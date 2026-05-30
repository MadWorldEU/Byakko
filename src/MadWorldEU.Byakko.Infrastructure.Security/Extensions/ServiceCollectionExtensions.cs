namespace MadWorldEU.Byakko.Extensions;

/// <summary>Registers security infrastructure services with the dependency injection container.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EncryptionOptions>(options =>
            configuration.GetSection(EncryptionOptions.SectionName).Bind(options));

        services.AddSingleton<IEncryptionService, EncryptionService>();

        return services;
    }
}