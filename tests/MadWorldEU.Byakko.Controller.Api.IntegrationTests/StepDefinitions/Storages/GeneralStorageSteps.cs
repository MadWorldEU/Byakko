namespace MadWorldEU.Byakko.StepDefinitions.Storages;

[Binding]
public sealed class GeneralStorageSteps(ScenarioContext scenarioContext)
{
    private const string AssetIdKey = "AssetId";
    private const string StorageStatisticsKey = "StorageStatistics";

    [Given("I have uploaded content with name {string} for the created asset")]
    public async Task GivenIHaveUploadedContentWithNameForTheCreatedAsset(string fileName)
    {
        var client = scenarioContext.Get<HttpClient>();
        var assetId = scenarioContext.Get<Guid>(AssetIdKey);
        var fileContent = new ByteArrayContent("Hello, World!"u8.ToArray());
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", fileName);
        var response = await client.PutAsync($"/assets/{assetId}/content", formData);
        response.EnsureSuccessStatusCode();
    }

    [When("I request the storage statistics")]
    public async Task WhenIRequestTheStorageStatistics()
    {
        var client = scenarioContext.Get<HttpClient>();
        var response = await client.GetAsync("/storage/statistics");
        scenarioContext.Set(response, ScenarioContextKeys.LastResponse);

        var statistics = await response.Content.ReadFromJsonAsync<GetStorageStatisticsResponse>();
        scenarioContext.Set(statistics, StorageStatisticsKey);
    }

    [Then("the storage statistics total files should be at least {int}")]
    public void ThenTheStorageStatisticsTotalFilesShouldBeAtLeast(int minFiles)
    {
        var statistics = scenarioContext.Get<GetStorageStatisticsResponse>(StorageStatisticsKey);
        statistics.ShouldNotBeNull();
        statistics.TotalFiles.ShouldBeGreaterThanOrEqualTo(minFiles);
    }

    [Then("the storage statistics total bytes should be greater than zero")]
    public void ThenTheStorageStatisticsTotalBytesShouldBeGreaterThanZero()
    {
        var statistics = scenarioContext.Get<GetStorageStatisticsResponse>(StorageStatisticsKey);
        statistics.ShouldNotBeNull();
        statistics.TotalBytes.ShouldBeGreaterThan(0);
    }
}