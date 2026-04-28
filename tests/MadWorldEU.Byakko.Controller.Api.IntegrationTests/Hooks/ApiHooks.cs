using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace MadWorldEU.Byakko.Hooks;

[Binding]
public class ApiHooks
{
    private static WebApplicationFactory<Program>? _factory;
    private static HttpClient? _client;

    private readonly ScenarioContext _scenarioContext;

    public ApiHooks(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [AfterTestRun]
    public static void AfterTestRun()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        _scenarioContext.Set(_client!);
    }
}