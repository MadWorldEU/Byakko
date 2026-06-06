namespace MadWorldEU.Byakko.StepDefinitions.Storages;

[Binding]
public sealed class AssetsSteps(ScenarioContext scenarioContext)
{
    private const string AssetIdKey = "AssetId";
    private const string CreatedAssetIdsKey = "CreatedAssetIds";

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
            var request = new CreateAssetRequest { Name = $"file-{i + 1}.txt", ContentType = "text/plain" };
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
}