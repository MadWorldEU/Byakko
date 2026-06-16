namespace MadWorldEU.Byakko.StepDefinitions.Audits;

[Binding]
public sealed class AuditLogsSteps(ScenarioContext scenarioContext)
{
    private const string AssetIdKey = "AssetId";

    [When("I request the audit logs for the created asset")]
    public async Task WhenIRequestTheAuditLogsForTheCreatedAsset()
    {
        var client = scenarioContext.Get<HttpClient>();
        var assetId = scenarioContext.Get<Guid>(AssetIdKey);
        var response = await client.GetAsync($"/audits/{assetId}");
        scenarioContext.Set(response, ScenarioContextKeys.LastResponse);
    }

    [Then("the audit logs should contain a {string} entry")]
    public async Task ThenTheAuditLogsShouldContainAnEntry(string action)
    {
        var response = scenarioContext.Get<HttpResponseMessage>(ScenarioContextKeys.LastResponse);
        var body = await response.Content.ReadFromJsonAsync<GetAuditLogsResponse>();
        body.ShouldNotBeNull();
        body.Logs.ShouldNotBeEmpty();
        body.Logs.ShouldContain(l => l.Action == action);
    }
}