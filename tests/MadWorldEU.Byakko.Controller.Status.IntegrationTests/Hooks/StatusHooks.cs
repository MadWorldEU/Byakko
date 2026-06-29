using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace MadWorldEU.Byakko.Hooks;

[Binding]
public sealed class StatusHooks(ScenarioContext scenarioContext)
{
    private static PostgreSqlContainer _postgres = null!;
    private static LocalStackContainer _localstack = null!;
    private static IContainer _mailpit = null!;
    private static string _mailpitApiUrl = null!;
    private static WireMockServer _healthMockServer = null!;
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

        _mailpit = new ContainerBuilder(DockerImages.Mailpit)
            .WithPortBinding(1025, true)
            .WithPortBinding(8025, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(r => r.ForPort(8025).ForPath("/api/v1/messages")))
            .Build();
        await _mailpit.StartAsync();

        var mailpitSmtpPort = _mailpit.GetMappedPublicPort(1025);
        _mailpitApiUrl = $"http://{_mailpit.Hostname}:{_mailpit.GetMappedPublicPort(8025)}";

        _healthMockServer = WireMockServer.Start();
        _healthMockServer
            .Given(Request.Create().WithPath("/health").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("Healthy"));

        var mockHealthUrl = $"{_healthMockServer.Urls[0]}/health";

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
                        ["HealthChecks:Api"] = mockHealthUrl,
                        ["HealthChecks:Admin"] = mockHealthUrl,
                        ["HealthChecks:Portal"] = mockHealthUrl,
                        ["HealthChecks:Authentication"] = mockHealthUrl,
                        ["Storage:Mode"] = "LocalStack",
                        ["Storage:AutoCreateBucket"] = "true",
                        ["MAILPIT_HOST"] = _mailpit.Hostname,
                        ["MAILPIT_PORT"] = mailpitSmtpPort.ToString()
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
        _healthMockServer.Stop();
        await _postgres.DisposeAsync();
        await _localstack.DisposeAsync();
        await _mailpit.DisposeAsync();
    }

    [BeforeScenario]
    public async Task BeforeScenario()
    {
        scenarioContext.Set(_client!);
        scenarioContext.Set(_factory!.Services, ScenarioContextKeys.ServiceProvider);
        scenarioContext.Set(_mailpitApiUrl, ScenarioContextKeys.MailpitApiUrl);
    }
}