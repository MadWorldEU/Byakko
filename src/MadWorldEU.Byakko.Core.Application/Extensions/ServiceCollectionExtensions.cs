namespace MadWorldEU.Byakko.Extensions;

/// <summary>Registers application use cases with the dependency injection container.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateAssetMetadataUseCase>();
        services.AddScoped<UploadAssetContentUseCase>();
        services.AddScoped<DownloadAssetContentUseCase>();

        return services;
    }
}