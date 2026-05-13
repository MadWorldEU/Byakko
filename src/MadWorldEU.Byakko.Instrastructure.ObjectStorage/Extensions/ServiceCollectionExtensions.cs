using MadWorldEU.Byakko.Connections;

namespace MadWorldEU.Byakko.Extensions;

/// <summary>Registers object storage infrastructure services with the dependency injection container.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddObjectStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IAmazonS3>(AmazonS3ClientFactory.Create(configuration));

        services.AddScoped<IContentStorage, ContentStorage>();

        return services;
    }
}