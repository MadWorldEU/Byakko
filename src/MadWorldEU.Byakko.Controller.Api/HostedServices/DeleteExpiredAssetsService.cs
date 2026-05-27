using MadWorldEU.Byakko.Configurations;
using Microsoft.Extensions.Options;
using NodaTime;

namespace MadWorldEU.Byakko.HostedServices;

/// <summary>Runs the expired asset cleanup use case once per day at the configured UTC hour.</summary>
internal sealed class DeleteExpiredAssetsService(
    IServiceScopeFactory scopeFactory,
    IOptions<CleanupSettings> settings,
    ILogger<DeleteExpiredAssetsService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = settings.Value.CalculateDelayUntilNextTrigger(clock);
            logger.LogInformation("Next expired asset content cleanup scheduled in {Delay}.", delay);

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
        logger.LogInformation("Starting expired asset content cleanup.");

        try
        {
            var useCase = scope.ServiceProvider.GetRequiredService<DeleteAllExpiredContentOfAssetsUseCase>();
            await useCase.ExecuteAsync();

            logger.LogInformation("Expired asset content cleanup completed.");
        }
        catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
        {
            logger.LogError(exception, "Unhandled exception during expired asset content cleanup.");
        }
    }
}