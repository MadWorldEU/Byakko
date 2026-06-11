using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MadWorldEU.Byakko.Hooks;

[Binding]
public sealed class StatusHooks(ScenarioContext scenarioContext)
{
    private static PostgreSqlContainer _postgres = null!;
    private static LocalStackContainer _localstack = null!;
    private static WebApplicationFactory<Program>? _factory;
    private static HttpClient? _client;

    [BeforeTestRun]
    public static async Task BeforeTestRun()
    {
        _postgres = new PostgreSqlBuilder(DockerImages.Postgres).Build();
        await _postgres.StartAsync();

        _localstack = new LocalStackBuilder(DockerImages.LocalStack)
            .WithEnvironment("SERVICES", "s3")
            .Build();
        await _localstack.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(host =>
            {
                host.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:byakko-db"] = _postgres.GetConnectionString(),
                        ["ConnectionStrings:localstack"] = _localstack.GetConnectionString(),
                        ["HealthChecks:Api"] = string.Empty,
                        ["HealthChecks:Admin"] = string.Empty,
                        ["HealthChecks:Portal"] = string.Empty,
                        ["HealthChecks:Authentication"] = string.Empty,
                        ["Storage:Mode"] = "LocalStack",
                        ["Storage:AutoCreateBucket"] = "true"
                    });
                });
            });

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ByakkoContext>();
        await context.Database.MigrateAsync();

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [AfterTestRun]
    public static async Task AfterTestRun()
    {
        _client?.Dispose();
        await (_factory?.DisposeAsync() ?? ValueTask.CompletedTask);
        await _postgres.DisposeAsync();
        await _localstack.DisposeAsync();
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        scenarioContext.Set(_client!);
        scenarioContext.Set(_factory!.Services, ScenarioContextKeys.ServiceProvider);
    }
}