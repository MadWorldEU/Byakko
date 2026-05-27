using MadWorldEU.Byakko.Configurations;
using Microsoft.Extensions.Options;
using NodaTime;

namespace MadWorldEU.Byakko.HostedServices;

/// <summary>Runs the expired asset metadata cleanup use case once per day at the configured UTC hour.</summary>
internal sealed class DeleteExpiredAssetMetaDataService(
    IServiceScopeFactory scopeFactory,
    IOptions<CleanupSettings> settings,
    ILogger<DeleteExpiredAssetMetaDataService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = settings.Value.CalculateDelayUntilNextTrigger(clock);
            logger.LogInformation("Next expired asset metadata cleanup scheduled in {Delay}.", delay);

            await Task.Delay(delay, stoppingToken);

            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            await RunCleanupAsync(scope, stoppingToken);
        }
    }

    private async Task RunCleanupAsync(IServiceScope scope, CancellationToken stoppingToken)
    {
        try
        {
            var useCase = scope.ServiceProvider.GetRequiredService<DeleteAllExpiredMetaDataAssetsUseCase>();
            await useCase.ExecuteAsync();
        }
        catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
        {
            logger.LogError(exception, "Unhandled exception during expired asset metadata cleanup.");
        }
    }
}