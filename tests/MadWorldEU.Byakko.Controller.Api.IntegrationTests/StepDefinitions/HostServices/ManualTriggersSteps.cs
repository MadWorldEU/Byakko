using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace MadWorldEU.Byakko.StepDefinitions.HostServices;

[Binding]
public sealed class ManualTriggersSteps(ScenarioContext scenarioContext)
{
    private const string ContentAssetIdKey = "ContentAssetId";
    private const string MetadataAssetIdKey = "MetadataAssetId";
    private const string AccountAssetIdKey = "AccountAssetId";

    [Given("I have set up an expired asset with uploaded content")]
    public async Task GivenIHaveSetUpAnExpiredAssetWithUploadedContent()
    {
        var client = scenarioContext.Get<HttpClient>();

        var createResponse = await client.PostAsJsonAsync("/assets", new CreateAssetRequest
        {
            Name = "expired-file.txt",
            ContentType = "text/plain",
            ExpiresInDays = 30
        });
        createResponse.EnsureSuccessStatusCode();
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateAssetResponse>();
        var assetId = createResult!.Id;
        scenarioContext.Set(assetId, ContentAssetIdKey);

        var fileContent = new ByteArrayContent("Expired content"u8.ToArray());
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "expired-file.txt");
        var uploadResponse = await client.PutAsync($"/assets/{assetId}/content", formData);
        uploadResponse.EnsureSuccessStatusCode();

        var services = scenarioContext.Get<IServiceProvider>(ScenarioContextKeys.ServiceProvider);
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ByakkoContext>();
        var pastDate = Instant.FromUtc(2020, 1, 1, 0, 0);
        var idValueObject = Id.Create(assetId).Value;
        await context.Assets
            .Where(a => a.Id == idValueObject)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.ExpiresAt, pastDate));
    }

    [Given("I have set up a soft-deleted asset")]
    public async Task GivenIHaveSetUpASoftDeletedAsset()
    {
        var client = scenarioContext.Get<HttpClient>();

        var createResponse = await client.PostAsJsonAsync("/assets", new CreateAssetRequest
        {
            Name = "deleted-file.txt",
            ContentType = "text/plain",
            ExpiresInDays = 30
        });
        createResponse.EnsureSuccessStatusCode();
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateAssetResponse>();
        var assetId = createResult!.Id;
        scenarioContext.Set(assetId, MetadataAssetIdKey);

        var services = scenarioContext.Get<IServiceProvider>(ScenarioContextKeys.ServiceProvider);
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ByakkoContext>();
        var overAYearAgo = Instant.FromUtc(2020, 1, 1, 0, 0);
        var idValueObject = Id.Create(assetId).Value;
        await context.Assets
            .Where(a => a.Id == idValueObject)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.DeletedAt, overAYearAgo));
    }

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

    [Given("I have registered a user in the authentication server")]
    public async Task GivenIHaveRegisteredAUserInTheAuthenticationServer()
    {
        var keycloakAdmin = scenarioContext.Get<KeycloakAdminTestClient>(ScenarioContextKeys.KeycloakAdmin);
        var userId = scenarioContext.Get<string>(ScenarioContextKeys.AccountUserId);
        await keycloakAdmin.CreateUserAsync("MadWorld", userId);
    }

    [Given("I have created an asset for the account")]
    public async Task GivenIHaveCreatedAnAssetForTheAccount()
    {
        var client = scenarioContext.Get<HttpClient>();
        var response = await client.PostAsJsonAsync("/assets", new CreateAssetRequest
        {
            Name = "account-file.txt",
            ContentType = "text/plain",
            ExpiresInDays = 30
        });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CreateAssetResponse>();
        scenarioContext.Set(result!.Id, AccountAssetIdKey);
    }

    [When("I trigger the account deletion cleanup")]
    public async Task WhenITriggerTheAccountDeletionCleanup()
    {
        var client = scenarioContext.Get<HttpClient>();
        var response = await client.PostAsync("/host-services/manual-triggers/clean-up/accounts", null);
        scenarioContext.Set(response, ScenarioContextKeys.LastResponse);
    }

    [Then("the user should be deleted from the authentication server")]
    public async Task ThenTheUserShouldBeDeletedFromTheAuthenticationServer()
    {
        var keycloakAdmin = scenarioContext.Get<KeycloakAdminTestClient>(ScenarioContextKeys.KeycloakAdmin);
        var userId = scenarioContext.Get<string>(ScenarioContextKeys.AccountUserId);
        var exists = await keycloakAdmin.UserExistsAsync("MadWorld", userId);
        exists.ShouldBeFalse();
    }

    [Then("the asset should be permanently deleted")]
    public async Task ThenTheAssetShouldBePermanentlyDeleted()
    {
        var assetId = scenarioContext.Get<Guid>(AccountAssetIdKey);
        var client = scenarioContext.Get<HttpClient>();
        var response = await client.GetAsync($"/assets/{assetId}");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Then("the expired asset content should be marked as deleted")]
    public async Task ThenTheExpiredAssetContentShouldBeMarkedAsDeleted()
    {
        var assetId = scenarioContext.Get<Guid>(ContentAssetIdKey);
        var services = scenarioContext.Get<IServiceProvider>(ScenarioContextKeys.ServiceProvider);
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ByakkoContext>();
        var idValueObject = Id.Create(assetId).Value;
        var asset = await context.Assets.FirstOrDefaultAsync(a => a.Id == idValueObject);
        asset.ShouldNotBeNull();
        asset.DeletedAt.ShouldNotBeNull();
    }

    [Then("the soft-deleted asset should be permanently removed")]
    public async Task ThenTheSoftDeletedAssetShouldBePermanentlyRemoved()
    {
        var assetId = scenarioContext.Get<Guid>(MetadataAssetIdKey);
        var client = scenarioContext.Get<HttpClient>();
        var response = await client.GetAsync($"/assets/{assetId}");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Then("the audit logs for the soft-deleted asset should be removed")]
    public async Task ThenTheAuditLogsForTheSoftDeletedAssetShouldBeRemoved()
    {
        var assetId = scenarioContext.Get<Guid>(MetadataAssetIdKey);
        var client = scenarioContext.Get<HttpClient>();
        var response = await client.GetAsync($"/audits/{assetId}");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<GetAuditLogsResponse>();
        body.ShouldNotBeNull();
        body.Logs.ShouldBeEmpty();
    }
}