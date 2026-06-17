namespace MadWorldEU.Byakko.StepDefinitions.Storages;

[Binding]
public sealed class AssetsSteps(ScenarioContext scenarioContext)
{
    private const string AssetIdKey = "AssetId";
    private const string CreatedAssetIdsKey = "CreatedAssetIds";
    private const string UploadLimitsKey = "UploadLimits";

    [Given("I am authenticated as a user")]
    public void GivenIAmAuthenticatedAsAUser()
    {
        var authenticatedClient = scenarioContext.Get<HttpClient>(ScenarioContextKeys.AuthenticatedClient);
        scenarioContext.Set(authenticatedClient);
    }

    [Given("I am authenticated as an administrator")]
    public void GivenIAmAuthenticatedAsAnAdministrator()
    {
        var authenticatedClient = scenarioContext.Get<HttpClient>(ScenarioContextKeys.AuthenticatedClient);
        scenarioContext.Set(authenticatedClient);
    }

    [Given("I have created {int} assets")]
    public async Task GivenIHaveCreatedAssets(int count)
    {
        var client = scenarioContext.Get<HttpClient>();
        var ids = new List<Guid>();

        for (var i = 0; i < count; i++)
        {
            var request = new CreateAssetRequest { Name = $"file-{i + 1}.txt", ContentType = "text/plain", Size = 10 };
            var response = await client.PostAsJsonAsync("/assets", request);
            response.EnsureSuccessStatusCode();
            var createResponse = await response.Content.ReadFromJsonAsync<CreateAssetResponse>();
            ids.Add(createResponse!.Id);
        }

        scenarioContext.Set(ids, CreatedAssetIdsKey);
    }

    [When("I request page {int} of all assets")]
    public async Task WhenIRequestPageOfAllAssets(int page)
    {
        var client = scenarioContext.Get<HttpClient>();
        var response = await client.GetAsync($"/assets?page={page}");
        scenarioContext.Set(response, ScenarioContextKeys.LastResponse);
    }

    [When("I request page {int} of my assets")]
    public async Task WhenIRequestPageOfMyAssets(int page)
    {
        var client = scenarioContext.Get<HttpClient>();
        var response = await client.GetAsync($"/assets/me?page={page}");
        scenarioContext.Set(response, ScenarioContextKeys.LastResponse);
    }

    [Then("the response should contain the created assets")]
    public async Task ThenTheResponseShouldContainTheCreatedAssets()
    {
        var body = await scenarioContext.Get<HttpResponseMessage>(ScenarioContextKeys.LastResponse)
            .Content.ReadFromJsonAsync<GetAssetsMetadataResponse>();

        var createdIds = scenarioContext.Get<List<Guid>>(CreatedAssetIdsKey);
        body.ShouldNotBeNull();
        body.TotalCount.ShouldBeGreaterThanOrEqualTo(createdIds.Count);
        foreach (var id in createdIds)
        {
            body.Assets.ShouldContain(a => a.Id == id);
        }
    }

    [Given("I have created an asset with name {string} and content type {string}")]
    public async Task GivenIHaveCreatedAnAsset(string name, string contentType)
    {
        var client = scenarioContext.Get<HttpClient>();
        var request = new CreateAssetRequest { Name = name, ContentType = contentType };
        var response = await client.PostAsJsonAsync("/assets", request);
        response.EnsureSuccessStatusCode();
        var createResponse = await response.Content.ReadFromJsonAsync<CreateAssetResponse>();
        scenarioContext.Set(createResponse!.Id, AssetIdKey);
    }

    [Given("I have uploaded content for the created asset")]
    public async Task GivenIHaveUploadedContentForTheCreatedAsset()
    {
        var client = scenarioContext.Get<HttpClient>();
        var assetId = scenarioContext.Get<Guid>(AssetIdKey);
        var fileContent = new ByteArrayContent("Hello, World!"u8.ToArray());
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "delete-test.txt");
        var response = await client.PutAsync($"/assets/{assetId}/content", formData);
        response.EnsureSuccessStatusCode();
    }

    [When("I retrieve the metadata of the created asset")]
    public async Task WhenIRetrieveTheMetadataOfTheCreatedAsset()
    {
        var client = scenarioContext.Get<HttpClient>();
        var assetId = scenarioContext.Get<Guid>(AssetIdKey);
        var response = await client.GetAsync($"/assets/{assetId}");
        scenarioContext.Set(response, ScenarioContextKeys.LastResponse);
    }

    [Then("the expire date of the created asset should be in the past")]
    public async Task ThenTheExpireDateOfTheCreatedAssetShouldBeInThePast()
    {
        var response = scenarioContext.Get<HttpResponseMessage>(ScenarioContextKeys.LastResponse);
        var metadata = await response.Content.ReadFromJsonAsync<GetAssetMetadataResponse>();
        metadata.ShouldNotBeNull();
        metadata.ExpiresAt.ShouldBeLessThanOrEqualTo(DateTimeOffset.UtcNow);
    }

    [When("I delete the content of the created asset")]
    public async Task WhenIDeleteTheContentOfTheCreatedAsset()
    {
        var client = scenarioContext.Get<HttpClient>();
        var assetId = scenarioContext.Get<Guid>(AssetIdKey);
        var response = await client.DeleteAsync($"/assets/{assetId}/content");
        scenarioContext.Set(response, ScenarioContextKeys.LastResponse);
    }

    [When("I delete my own asset content")]
    public async Task WhenIDeleteMyOwnAssetContent()
    {
        var client = scenarioContext.Get<HttpClient>();
        var assetId = scenarioContext.Get<Guid>(AssetIdKey);
        var response = await client.DeleteAsync($"/assets/me/{assetId}/content");
        scenarioContext.Set(response, ScenarioContextKeys.LastResponse);
    }

    [When("I upload content for the created asset")]
    public async Task WhenIUploadContentForTheCreatedAsset()
    {
        var client = scenarioContext.Get<HttpClient>();
        var assetId = scenarioContext.Get<Guid>(AssetIdKey);
        var fileContent = new ByteArrayContent("Hello, World!"u8.ToArray());
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "test-file.txt");
        var response = await client.PutAsync($"/assets/{assetId}/content", formData);
        scenarioContext.Set(response, ScenarioContextKeys.LastResponse);
    }

    [When("I download the content of the created asset")]
    public async Task WhenIDownloadTheContentOfTheCreatedAsset()
    {
        var client = scenarioContext.Get<HttpClient>();
        var assetId = scenarioContext.Get<Guid>(AssetIdKey);
        var response = await client.GetAsync($"/assets/{assetId}/content");
        scenarioContext.Set(response, ScenarioContextKeys.LastResponse);
    }

    [When("I request my upload limits")]
    public async Task WhenIRequestMyUploadLimits()
    {
        var client = scenarioContext.Get<HttpClient>();
        var response = await client.GetAsync("/assets/limits");
        scenarioContext.Set(response, ScenarioContextKeys.LastResponse);

        var limits = await response.Content.ReadFromJsonAsync<GetUserUploadLimitsResponse>();
        scenarioContext.Set(limits, UploadLimitsKey);
    }

    [Then("the upload limits should reflect my usage")]
    public void ThenTheUploadLimitsShouldReflectMyUsage()
    {
        var limits = scenarioContext.Get<GetUserUploadLimitsResponse>(UploadLimitsKey);
        limits.ShouldNotBeNull();
        limits.MaxFiles.ShouldBeGreaterThan(0);
        limits.MaxUploadSizeInBytes.ShouldBeGreaterThan(0);
        limits.ActiveFiles.ShouldBeGreaterThanOrEqualTo(1);
    }
}