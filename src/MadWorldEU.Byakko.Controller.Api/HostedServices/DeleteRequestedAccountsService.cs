using MadWorldEU.Byakko.Accounts;
using MadWorldEU.Byakko.Configurations;
using NodaTime;

namespace MadWorldEU.Byakko.HostedServices;

/// <summary>Runs the confirmed-deletion account cleanup use case once per day at the configured UTC hour.</summary>
internal sealed class DeleteRequestedAccountsService(
    IServiceScopeFactory scopeFactory,
    IOptions<CleanupSettings> settings,
    ILogger<DeleteRequestedAccountsService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = settings.Value.CalculateDelayUntilNextTrigger(clock);
            logger.LogInformation("Next account deletion cleanup scheduled in {Delay}.", delay);

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
        logger.LogInformation("Starting account deletion cleanup.");

        try
        {
            var useCase = scope.ServiceProvider.GetRequiredService<DeleteRequestedAccountsUseCase>();
            await useCase.ExecuteAsync();

            logger.LogInformation("Account deletion cleanup completed.");
        }
        catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
        {
            logger.LogError(exception, "Unhandled exception during account deletion cleanup.");
        }
    }
}