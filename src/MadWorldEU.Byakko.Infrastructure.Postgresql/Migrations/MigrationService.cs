namespace MadWorldEU.Byakko.Migrations;

/// <summary>Runs pending EF Core migrations on application startup when auto-migrate is enabled.</summary>
internal sealed class MigrationService(IServiceProvider services) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ByakkoContext>();
        await context.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}