using Microsoft.EntityFrameworkCore;

namespace MadWorldEU.Byakko.StepDefinitions.Accounts;

[Binding]
public sealed class AccountsSteps(ScenarioContext scenarioContext)
{

    [BeforeScenario(Order = 2)]
    public void BeforeScenario()
    {
        var factory = scenarioContext.Get<WebApplicationFactory<Program>>(ScenarioContextKeys.Factory);
        var uniqueUserId = Guid.NewGuid().ToString();

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwtToken.Create(uniqueUserId));
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "171.129.229.213");

        scenarioContext.Set(client);
        scenarioContext.Set(client, ScenarioContextKeys.AuthenticatedClient);
        scenarioContext.Set(uniqueUserId, ScenarioContextKeys.AccountUserId);
    }

    [Given("I have registered a user with username {string} in the authentication server")]
    public async Task GivenIHaveRegisteredAUserWithUsernameInTheAuthenticationServer(string username)
    {
        var keycloakAdmin = scenarioContext.Get<KeycloakAdminTestClient>(ScenarioContextKeys.KeycloakAdmin);
        var keycloakUserId = await keycloakAdmin.CreateUserAsync("MadWorld", username);

        var factory = scenarioContext.Get<WebApplicationFactory<Program>>(ScenarioContextKeys.Factory);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwtToken.Create(keycloakUserId));
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "171.129.229.213");

        scenarioContext.Set(client);
        scenarioContext.Set(client, ScenarioContextKeys.AuthenticatedClient);
        scenarioContext.Set(keycloakUserId, ScenarioContextKeys.AccountUserId);
    }

    [Given("I have created my account")]
    public async Task GivenIHaveCreatedMyAccount()
    {
        var client = scenarioContext.Get<HttpClient>();
        var response = await client.PostAsJsonAsync("/accounts/me", new { });
        response.EnsureSuccessStatusCode();
    }

    [Given("I have requested deletion of my account")]
    public async Task GivenIHaveRequestedDeletionOfMyAccount()
    {
        var client = scenarioContext.Get<HttpClient>();
        var response = await client.PostAsJsonAsync("/accounts/me/deletion-request", new { });
        response.EnsureSuccessStatusCode();
    }

    [Given("I have confirmed the deletion of the account")]
    public async Task GivenIHaveConfirmedTheDeletionOfTheAccount()
    {
        var client = scenarioContext.Get<HttpClient>();
        var userId = scenarioContext.Get<string>(ScenarioContextKeys.AccountUserId);
        var response = await client.PostAsJsonAsync($"/accounts/{userId}/confirm-deletion", new { });
        response.EnsureSuccessStatusCode();
    }

    [When("I request the accounts with deletion requested for page {int}")]
    public async Task WhenIRequestTheAccountsWithDeletionRequestedForPage(int page)
    {
        var client = scenarioContext.Get<HttpClient>();
        var response = await client.GetAsync($"/accounts?page={page}");
        scenarioContext.Set(response, ScenarioContextKeys.LastResponse);
    }

    [Then("the response should contain at least one account with deletion requested")]
    public async Task ThenTheResponseShouldContainAtLeastOneAccountWithDeletionRequested()
    {
        var response = scenarioContext.Get<HttpResponseMessage>(ScenarioContextKeys.LastResponse);
        var body = await response.Content.ReadFromJsonAsync<GetAccountsPendingDeletionResponse>();
        body.ShouldNotBeNull();
        body.TotalCount.ShouldBeGreaterThan(0);

        var userId = scenarioContext.Get<string>(ScenarioContextKeys.AccountUserId);
        var account = body.Accounts.First(a => a.UserId.ToString() == userId);
        account.Status.ShouldBe("DeletionRequested");
        account.Username.ShouldBe("johnsmith");
    }

    [When("I cancel the deletion request of the account")]
    public async Task WhenICancelTheDeletionRequestOfTheAccount()
    {
        var client = scenarioContext.Get<HttpClient>();
        var userId = scenarioContext.Get<string>(ScenarioContextKeys.AccountUserId);
        var response = await client.PostAsJsonAsync($"/accounts/{userId}/cancel-deletion-request", new { });
        scenarioContext.Set(response, ScenarioContextKeys.LastResponse);
    }

    [Then("the cancel deletion request response should be returned")]
    public async Task ThenTheCancelDeletionRequestResponseShouldBeReturned()
    {
        var response = scenarioContext.Get<HttpResponseMessage>(ScenarioContextKeys.LastResponse);
        var body = await response.Content.ReadFromJsonAsync<CancelDeletionRequestAccountResponse>();
        body.ShouldNotBeNull();
        body.UserId.ShouldNotBe(Guid.Empty);
    }

    [When("I confirm the deletion of the account")]
    public async Task WhenIConfirmTheDeletionOfTheAccount()
    {
        var client = scenarioContext.Get<HttpClient>();
        var userId = scenarioContext.Get<string>(ScenarioContextKeys.AccountUserId);
        var response = await client.PostAsJsonAsync($"/accounts/{userId}/confirm-deletion", new { });
        scenarioContext.Set(response, ScenarioContextKeys.LastResponse);
    }

    [Then("the account should have status DeletionConfirmed")]
    public async Task ThenTheAccountShouldHaveStatusDeletionConfirmed()
    {
        var client = scenarioContext.Get<HttpClient>();
        var response = await client.GetAsync("/accounts/me");
        var body = await response.Content.ReadFromJsonAsync<GetMyAccountResponse>();
        body.ShouldNotBeNull();
        body.Status.ShouldBe("DeletionConfirmed");
    }

    [Then("the account should have status Deleted")]
    public async Task ThenTheAccountShouldHaveStatusDeleted()
    {
        var userId = scenarioContext.Get<string>(ScenarioContextKeys.AccountUserId);
        var services = scenarioContext.Get<IServiceProvider>(ScenarioContextKeys.ServiceProvider);
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ByakkoContext>();
        var userIdValueObject = UserId.Create(userId).Value;
        var account = await context.Accounts.FirstOrDefaultAsync(a => a.UserId == userIdValueObject);
        account.ShouldNotBeNull();
        account.Status.ShouldBe(AccountStatus.Deleted);
    }

    [Then("the confirm deletion response should be returned")]
    public async Task ThenTheConfirmDeletionResponseShouldBeReturned()
    {
        var response = scenarioContext.Get<HttpResponseMessage>(ScenarioContextKeys.LastResponse);
        var body = await response.Content.ReadFromJsonAsync<ConfirmDeletionAccountResponse>();
        body.ShouldNotBeNull();
        body.UserId.ShouldNotBe(Guid.Empty);
    }

    [When("I create my account")]
    public async Task WhenICreateMyAccount()
    {
        var client = scenarioContext.Get<HttpClient>();
        var response = await client.PostAsJsonAsync("/accounts/me", new { });
        scenarioContext.Set(response, ScenarioContextKeys.LastResponse);
    }

    [Then("the created account should be returned")]
    public async Task ThenTheCreatedAccountShouldBeReturned()
    {
        var response = scenarioContext.Get<HttpResponseMessage>(ScenarioContextKeys.LastResponse);
        var body = await response.Content.ReadFromJsonAsync<CreateMyAccountResponse>();
        body.ShouldNotBeNull();
        body.UserId.ShouldNotBe(Guid.Empty);
    }

    [When("I request deletion of my account")]
    public async Task WhenIRequestDeletionOfMyAccount()
    {
        var client = scenarioContext.Get<HttpClient>();
        var response = await client.PostAsJsonAsync("/accounts/me/deletion-request", new { });
        scenarioContext.Set(response, ScenarioContextKeys.LastResponse);
    }

    [Then("the deletion request account should be returned")]
    public async Task ThenTheDeletionRequestAccountShouldBeReturned()
    {
        var response = scenarioContext.Get<HttpResponseMessage>(ScenarioContextKeys.LastResponse);
        var body = await response.Content.ReadFromJsonAsync<RequestDeletionMyAccountResponse>();
        body.ShouldNotBeNull();
        body.UserId.ShouldNotBe(Guid.Empty);
    }

    [When("I request my account")]
    public async Task WhenIRequestMyAccount()
    {
        var client = scenarioContext.Get<HttpClient>();
        var response = await client.GetAsync("/accounts/me");
        scenarioContext.Set(response, ScenarioContextKeys.LastResponse);
    }

    [Then("the account should have deletion requested")]
    public async Task ThenTheAccountShouldHaveDeletionRequested()
    {
        var response = scenarioContext.Get<HttpResponseMessage>(ScenarioContextKeys.LastResponse);
        var body = await response.Content.ReadFromJsonAsync<GetMyAccountResponse>();
        body.ShouldNotBeNull();
        body.Status.ShouldBe("DeletionRequested");
    }

    [Then("the account should be returned with no deletion requested")]
    public async Task ThenTheAccountShouldBeReturnedWithNoDeletionRequested()
    {
        var response = scenarioContext.Get<HttpResponseMessage>(ScenarioContextKeys.LastResponse);
        var body = await response.Content.ReadFromJsonAsync<GetMyAccountResponse>();
        body.ShouldNotBeNull();
        body.UserId.ShouldNotBe(Guid.Empty);
        body.Status.ShouldBe("Active");
    }
}