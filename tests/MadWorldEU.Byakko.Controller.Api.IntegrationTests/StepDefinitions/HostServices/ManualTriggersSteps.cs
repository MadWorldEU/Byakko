namespace MadWorldEU.Byakko.StepDefinitions.HostServices;

[Binding]
public sealed class ManualTriggersSteps(ScenarioContext scenarioContext)
{
    [When("I trigger the expired asset content cleanup")]
    public async Task WhenITriggerTheExpiredAssetContentCleanup()
    {
        var client = scenarioContext.Get<HttpClient>();
        var response = await client.PostAsync("/host-services/manual-triggers/clean-up/assets-content", null);
        scenarioContext.Set(response, ScenarioContextKeys.LastResponse);
    }

    [When("I trigger the expired asset metadata cleanup")]
    public async Task WhenITriggerTheExpiredAssetMetadataCleanup()
    {
        var client = scenarioContext.Get<HttpClient>();
        var response = await client.PostAsync("/host-services/manual-triggers/clean-up/assets-metadata", null);
        scenarioContext.Set(response, ScenarioContextKeys.LastResponse);
    }
}