using Microsoft.Extensions.Configuration;

namespace MadWorldEU.Byakko.Extensions;

/// <summary>Registers application use cases with the dependency injection container.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AssetSettings>(options =>
            configuration.GetSection(AssetSettings.Key).Bind(options));

        services.AddScoped<CreateAssetMetadataUseCase>();
        services.AddScoped<DeleteAllExpiredContentOfAssetsUseCase>();
        services.AddScoped<DeleteAllExpiredMetaDataAssetsUseCase>();
        services.AddScoped<GetAssetMetadataUseCase>();
        services.AddScoped<GetAssetsMetaDataUseCase>();
        services.AddScoped<UploadAssetContentUseCase>();
        services.AddScoped<DownloadAssetContentUseCase>();

        return services;
    }
}