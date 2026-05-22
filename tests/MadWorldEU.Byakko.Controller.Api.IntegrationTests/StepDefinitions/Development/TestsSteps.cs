namespace MadWorldEU.Byakko.StepDefinitions.Development;

[Binding]
public sealed class TestsSteps(ScenarioContext scenarioContext)
{
    [When("I send a GET request to {string}")]
    public async Task WhenISendAGetRequestTo(string endpoint)
    {
        var client = scenarioContext.Get<HttpClient>();
        var response = await client.GetAsync(endpoint);
        scenarioContext.Set(response, ScenarioContextKeys.LastResponse);
    }

    [Then("the response status code should be {int}")]
    public void ThenTheResponseStatusCodeShouldBe(int statusCode)
    {
        var response = scenarioContext.Get<HttpResponseMessage>(ScenarioContextKeys.LastResponse);
        response.StatusCode.ShouldBe((HttpStatusCode)statusCode);
    }

    [Then("the response body should be {string}")]
    public async Task ThenTheResponseBodyShouldBe(string expectedBody)
    {
        var response = scenarioContext.Get<HttpResponseMessage>(ScenarioContextKeys.LastResponse);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldBe(expectedBody);
    }
}