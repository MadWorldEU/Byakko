using System.Net;
using Reqnroll;
using Shouldly;

namespace MadWorldEU.Byakko.StepDefinitions.Development;

[Binding]
public class TestsSteps
{
    private readonly ScenarioContext _scenarioContext;
    private HttpResponseMessage? _response;

    public TestsSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [When("I send a GET request to {string}")]
    public async Task WhenISendAGetRequestTo(string endpoint)
    {
        var client = _scenarioContext.Get<HttpClient>();
        _response = await client.GetAsync(endpoint);
    }

    [Then("the response status code should be {int}")]
    public void ThenTheResponseStatusCodeShouldBe(int statusCode)
    {
        _response!.StatusCode.ShouldBe((HttpStatusCode)statusCode);
    }

    [Then("the response body should be {string}")]
    public async Task ThenTheResponseBodyShouldBe(string expectedBody)
    {
        var body = await _response!.Content.ReadAsStringAsync();
        body.ShouldBe(expectedBody);
    }
}