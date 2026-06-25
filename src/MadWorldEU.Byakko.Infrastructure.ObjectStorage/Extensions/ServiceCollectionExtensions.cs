namespace MadWorldEU.Byakko.Extensions;

/// <summary>Registers object storage infrastructure services with the dependency injection container.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddObjectStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<StorageOptions>(options =>
            configuration.GetSection(StorageOptions.SectionName).Bind(options));

        services.AddSingleton<IAmazonS3>(sp => AmazonS3ClientFactory.Create(sp.GetRequiredService<IConfiguration>()));

        services.AddScoped<IContentStorage, ContentStorage>();
        services.AddHostedService<BucketInitializer>();

        return services;
    }
}