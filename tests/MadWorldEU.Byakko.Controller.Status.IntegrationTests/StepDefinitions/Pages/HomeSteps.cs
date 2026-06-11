using System.Text.RegularExpressions;

namespace MadWorldEU.Byakko.StepDefinitions.Pages;

[Binding]
public sealed class HomeSteps(ScenarioContext scenarioContext)
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

    [Then("the response body should contain {string}")]
    public async Task ThenTheResponseBodyShouldContain(string expectedContent)
    {
        var response = scenarioContext.Get<HttpResponseMessage>(ScenarioContextKeys.LastResponse);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain(expectedContent);
    }

    [Then("the service {string} should have status {string}")]
    public async Task ThenTheServiceShouldHaveStatus(string serviceName, string status)
    {
        var response = scenarioContext.Get<HttpResponseMessage>(ScenarioContextKeys.LastResponse);
        var body = await response.Content.ReadAsStringAsync();
        var pattern = $@"{Regex.Escape(serviceName)}[\s\S]{{0,300}}{Regex.Escape(status)}";
        Regex.IsMatch(body, pattern).ShouldBeTrue($"Expected service '{serviceName}' to have status '{status}'");
    }
}