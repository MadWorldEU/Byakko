using Microsoft.EntityFrameworkCore;

namespace MadWorldEU.Byakko.Hooks;

[Binding]
public sealed class ApiHooks(ScenarioContext scenarioContext)
{
    private static PostgreSqlContainer _postgres = null!;
    private static MinioContainer _minio = null!;
    private static WebApplicationFactory<Program>? _factory;
    private static HttpClient? _client;

    [BeforeTestRun]
    public static async Task BeforeTestRun()
    {
        _postgres = new PostgreSqlBuilder("postgres:15.1").Build();
        await _postgres.StartAsync();

        _minio = new MinioBuilder("minio/minio:RELEASE.2023-01-31T02-24-19Z").Build();
        await _minio.StartAsync().WaitAsync(CancellationToken.None);

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(host =>
            {
                host.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:byakko-db"] = _postgres.GetConnectionString(),
                        ["ConnectionStrings:minio"] = $"Endpoint={_minio.GetConnectionString()};AccessKey={_minio.GetAccessKey()};SecretKey={_minio.GetSecretKey()}"
                    });
                });
            });

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ByakkoContext>();
        await context.Database.MigrateAsync();

        _client = _factory.CreateClient();
    }

    [AfterTestRun]
    public static async Task AfterTestRun()
    {
        _client?.Dispose();
        await (_factory?.DisposeAsync() ?? ValueTask.CompletedTask);
        await _postgres.DisposeAsync();
        await _minio.DisposeAsync();
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        scenarioContext.Set(_client!);
    }
}