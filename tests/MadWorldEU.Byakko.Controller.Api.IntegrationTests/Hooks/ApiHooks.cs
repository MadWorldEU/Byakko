using Microsoft.EntityFrameworkCore;

namespace MadWorldEU.Byakko.Hooks;

[Binding]
public sealed class ApiHooks
{
    private static PostgreSqlContainer _postgres = null!;
    private static WebApplicationFactory<Program>? _factory;
    private static HttpClient? _client;

    private readonly ScenarioContext _scenarioContext;

    public ApiHooks(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeTestRun]
    public static async Task BeforeTestRun()
    {
        _postgres = new PostgreSqlBuilder().Build();
        await _postgres.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(host =>
            {
                host.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<ByakkoContext>));
                    if (descriptor is not null)
                        services.Remove(descriptor);

                    services.AddDbContext<ByakkoContext>(options =>
                        options.UseNpgsql(_postgres.GetConnectionString()));
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
        _factory?.Dispose();
        await _postgres.DisposeAsync();
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        _scenarioContext.Set(_client!);
    }
}