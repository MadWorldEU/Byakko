using MadWorldEU.Byakko.Accounts;
using MadWorldEU.Byakko.Audits;
using MadWorldEU.Byakko.AuthenticationServers;
using MadWorldEU.Byakko.Correspondences;
using MadWorldEU.Byakko.DomainDrivenDevelopment;
using Microsoft.Extensions.Configuration;

namespace MadWorldEU.Byakko.Extensions;

/// <summary>Registers application use cases with the dependency injection container.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAssets(configuration);
        services.AddAccounts();
        services.AddAudits();
        services.AddAuthenticationServers();
        services.AddCorrespondences();
        
        return services;
    }

    private static void AddAssets(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AssetSettings>(options =>
            configuration.GetSection(AssetSettings.Key).Bind(options));

        services.AddSingleton<IAssetMetrics, AssetMetrics>();

        services.AddScoped<IDomainEventHandler<AccountDeletedEvent>, AssetAccountEventHandler>();
        
        services.AddScoped<CreateAssetMetadataUseCase>();
        services.AddScoped<DeleteAllExpiredContentOfAssetsUseCase>();
        services.AddScoped<DeleteAllExpiredMetaDataAssetsUseCase>();
        services.AddScoped<DeleteContentOfAssetUseCase>();
        services.AddScoped<DeleteMyAssetContentUseCase>();
        services.AddScoped<DownloadAssetContentUseCase>();
        services.AddScoped<GetAssetMetadataUseCase>();
        services.AddScoped<GetAssetsMetaDataUseCase>();
        services.AddScoped<GetStorageStatisticsUseCase>();
        services.AddScoped<GetUserUploadLimitsUseCase>();
        services.AddScoped<UploadAssetContentUseCase>();
    }

    private static void AddAccounts(this IServiceCollection services)
    {
        services.AddScoped<CancelDeletionRequestAccountUseCase>();
        services.AddScoped<ConfirmDeletionAccountUseCase>();
        services.AddScoped<CreateMyAccountUseCase>();
        services.AddScoped<DeleteRequestedAccountsUseCase>();
        services.AddScoped<GetMyAccountUseCase>();
        services.AddScoped<GetAccountsPendingDeletionUseCase>();
        services.AddScoped<RequestDeletionMyAccountUseCase>();
    }
    
    private static void AddAudits(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventHandler<AccountDeletedEvent>, AuditAccountEventHandler>();
        services.AddScoped<IDomainEventHandler<AssetMetaDataCreatedEvent>, AuditAssetsEventHandler>();
        services.AddScoped<IDomainEventHandler<AssetContentDeletedEvent>, AuditAssetsEventHandler>();
        services.AddScoped<IDomainEventHandler<AssetContentUploadedEvent>, AuditAssetsEventHandler>();

        services.AddScoped<GetAuditLogsUseCase>();
    }
    
    private static void AddAuthenticationServers(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventHandler<AccountDeletedEvent>, AuthenticationServerAccountEventHandler>();
    }
    
    private static void AddCorrespondences(this IServiceCollection services)
    {
        services.AddSingleton<ICorrespondenceMetrics, CorrespondenceMetrics>();
        services.AddScoped<SendFeedbackUseCase>();
    }
}