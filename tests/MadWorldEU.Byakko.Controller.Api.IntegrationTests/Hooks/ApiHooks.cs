using Microsoft.EntityFrameworkCore;

namespace MadWorldEU.Byakko.Hooks;

[Binding]
public sealed class ApiHooks(ScenarioContext scenarioContext)
{
    private const string TestAdminClientSecret = "test-admin-secret";
    private const string ManagedRealm = "MadWorld";

    private static KeycloakContainer _keycloak = null!;
    private static KeycloakAdminTestClient _keycloakAdmin = null!;
    private static PostgreSqlContainer _postgres = null!;
    private static LocalStackContainer _localstack = null!;
    private static IContainer _mailpit = null!;
    private static string _mailpitApiUrl = null!;
    private static WebApplicationFactory<Program>? _factory;
    private static HttpClient? _client;
    private static HttpClient? _authenticatedClient;

    [BeforeTestRun]
    public static async Task BeforeTestRun()
    {
        _keycloak = new KeycloakBuilder(DockerImages.Keycloak).Build();
        await _keycloak.StartAsync();

        var keycloakBaseUrl = $"http://{_keycloak.Hostname}:{_keycloak.GetMappedPublicPort(8080)}";
        _keycloakAdmin = new KeycloakAdminTestClient(keycloakBaseUrl, TestAdminClientSecret);
        await _keycloakAdmin.CreateRealmAsync(ManagedRealm);
        await _keycloakAdmin.CreateAdminServiceAccountAsync(TestAdminClientSecret, ManagedRealm);

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
                        ["Authentication:ValidateUser"] = "false",
                        ["Assets:MaxFilesEachUser"] = "1000",
                        ["MAILPIT_HOST"] = _mailpit.Hostname,
                        ["MAILPIT_PORT"] = mailpitSmtpPort.ToString(),
                        ["Logging:LogLevel:Microsoft.EntityFrameworkCore"] = "Warning",
                        ["KeyCloak:AuthServerUrl"] = $"http://{_keycloak.Hostname}:{_keycloak.GetMappedPublicPort(8080)}/",
                        ["KeyCloak:Resource"] = "madworld-admin-api",
                        ["KeyCloak:AdminClientSecret"] = TestAdminClientSecret,
                        ["KeyCloak:ManagedRealm"] = ManagedRealm
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
        await _mailpit.DisposeAsync();
        await _keycloak.DisposeAsync();
    }

    [BeforeScenario(Order = 1)]
    public Task BeforeScenario()
    {
        scenarioContext.Set(_client!);
        scenarioContext.Set(_authenticatedClient!, ScenarioContextKeys.AuthenticatedClient);
        scenarioContext.Set(_factory!, ScenarioContextKeys.Factory);
        scenarioContext.Set(_factory!.Services, ScenarioContextKeys.ServiceProvider);
        scenarioContext.Set(_mailpitApiUrl, ScenarioContextKeys.MailpitApiUrl);
        scenarioContext.Set(_keycloakAdmin, ScenarioContextKeys.KeycloakAdmin);

        return Task.CompletedTask;
    }
}