namespace MadWorldEU.Byakko.StepDefinitions.Correspondences;

[Binding]
[Scope(Feature = "Correspondences Endpoints")]
public sealed class CorrespondencesSteps(ScenarioContext scenarioContext)
{
    [When("I send feedback with email {string} and message {string}")]
    public async Task WhenISendFeedbackWithEmailAndMessage(string email, string message)
    {
        var client = scenarioContext.Get<HttpClient>();
        var request = new SendFeedbackRequest { Email = email, Message = message };
        var response = await client.PostAsJsonAsync("/correspondences/feedback", request);
        scenarioContext.Set(response, ScenarioContextKeys.LastResponse);
    }

    [Then("the feedback request should succeed")]
    public void ThenTheFeedbackRequestShouldSucceed()
    {
        var response = scenarioContext.Get<HttpResponseMessage>(ScenarioContextKeys.LastResponse);
        response.IsSuccessStatusCode.ShouldBeTrue();
    }

    [Then("the administrator should have received an email with subject {string}")]
    public async Task ThenTheAdministratorShouldHaveReceivedAnEmailWithSubject(string subject)
    {
        var mailpitUrl = scenarioContext.Get<string>(ScenarioContextKeys.MailpitApiUrl);
        using var httpClient = new HttpClient();
        var messages = await httpClient.GetFromJsonAsync<MailpitMessagesResponse>($"{mailpitUrl}/api/v1/messages");
        messages.ShouldNotBeNull();
        messages.Messages.ShouldContain(m => m.Subject == subject);
    }
}