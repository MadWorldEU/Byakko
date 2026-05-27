using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace MadWorldEU.Byakko.StepDefinitions.HostServices;

[Binding]
public sealed class ManualTriggersSteps(ScenarioContext scenarioContext)
{
    private const string ContentAssetIdKey = "ContentAssetId";
    private const string MetadataAssetIdKey = "MetadataAssetId";

    [Given("I have set up an expired asset with uploaded content")]
    public async Task GivenIHaveSetUpAnExpiredAssetWithUploadedContent()
    {
        var client = scenarioContext.Get<HttpClient>();

        var createResponse = await client.PostAsJsonAsync("/assets", new CreateAssetRequest
        {
            Name = "expired-file.txt",
            ContentType = "text/plain"
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
            ContentType = "text/plain"
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
}