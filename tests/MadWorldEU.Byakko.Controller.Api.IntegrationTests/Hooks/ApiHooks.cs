using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;

namespace MadWorldEU.Byakko.Hooks;

[Binding]
public sealed class ApiHooks(ScenarioContext scenarioContext)
{
    private static PostgreSqlContainer _postgres = null!;
    private static LocalStackContainer _localstack = null!;
    private static WebApplicationFactory<Program>? _factory;
    private static HttpClient? _client;
    private static HttpClient? _authenticatedClient;

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
                        ["RateLimiting:Enabled"] = "false",
                        ["Authentication:ValidateUser"] = "false"
                    });
                });
            });

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ByakkoContext>();
        await context.Database.MigrateAsync();

        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", "171.129.229.213");

        _authenticatedClient = _factory.CreateClient();
        _authenticatedClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwtToken.Create("b7c12261-3707-43f6-9403-045a41422f25"));
        _authenticatedClient.DefaultRequestHeaders.Add("X-Forwarded-For", "171.129.229.213");
    }

    [AfterTestRun]
    public static async Task AfterTestRun()
    {
        _client?.Dispose();
        _authenticatedClient?.Dispose();
        await (_factory?.DisposeAsync() ?? ValueTask.CompletedTask);
        await _postgres.DisposeAsync();
        await _localstack.DisposeAsync();
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        scenarioContext.Set(_client!);
        scenarioContext.Set(_authenticatedClient!, ScenarioContextKeys.AuthenticatedClient);
        scenarioContext.Set(_factory!.Services, ScenarioContextKeys.ServiceProvider);
    }
}