using MadWorldEU.Byakko.Audits;
using MadWorldEU.Byakko.DomainDrivenDevelopment;
using Microsoft.Extensions.Configuration;

namespace MadWorldEU.Byakko.Extensions;

/// <summary>Registers application use cases with the dependency injection container.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAssets(configuration);
        services.AddAudits();
        
        return services;
    }

    private static void AddAssets(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AssetSettings>(options =>
            configuration.GetSection(AssetSettings.Key).Bind(options));

        services.AddScoped<CreateAssetMetadataUseCase>();
        services.AddScoped<DeleteAllExpiredContentOfAssetsUseCase>();
        services.AddScoped<DeleteAllExpiredMetaDataAssetsUseCase>();
        services.AddScoped<DeleteContentOfAssetUseCase>();
        services.AddScoped<DownloadAssetContentUseCase>();
        services.AddScoped<GetAssetMetadataUseCase>();
        services.AddScoped<GetAssetsMetaDataUseCase>();
        services.AddScoped<GetStorageStatisticsUseCase>();
        services.AddScoped<GetUserUploadLimitsUseCase>();
        services.AddScoped<UploadAssetContentUseCase>();
    }
    
    private static void AddAudits(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventHandler<AssetMetaDataCreatedEvent>, AuditAssetsHandler>();
        services.AddScoped<IDomainEventHandler<AssetContentDeletedEvent>, AuditAssetsHandler>();
        services.AddScoped<IDomainEventHandler<AssetContentUploadedEvent>, AuditAssetsHandler>();

        services.AddScoped<GetAuditLogsUseCase>();
    }
}