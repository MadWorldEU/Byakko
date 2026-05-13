using MadWorldEU.Byakko.Connections;

namespace MadWorldEU.Byakko.Extensions;

/// <summary>Registers object storage infrastructure services with the dependency injection container.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddObjectStorage(this IServiceCollection services)
    {
        services.AddSingleton<IAmazonS3>(sp => AmazonS3ClientFactory.Create(sp.GetRequiredService<IConfiguration>()));

        services.AddScoped<IContentStorage, ContentStorage>();
        services.AddHostedService<BucketInitializer>();

        return services;
    }
}