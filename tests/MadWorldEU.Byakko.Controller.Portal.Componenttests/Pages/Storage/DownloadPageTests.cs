namespace MadWorldEU.Byakko.Pages.Storage;

/// <summary>Component tests for the Download page.</summary>
public sealed class DownloadPageTests
{
    [Test]
    public void OnInitializedAsync_WhenAssetExists_ShouldShowFileMetadataAndDownloadLink()
    {
        var assetId = Guid.NewGuid();

        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath($"/assets/{assetId}").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new GetAssetMetadataResponse
                {
                    Id = assetId,
                    Name = "test-file.txt",
                    ContentType = "text/plain",
                    CreatedAt = "2026-05-22T12:00:00Z"
                }));

        using var ctx = new BunitContext();
        ctx.Services.AddHttpClient(HttpClients.ApiAnonymous, client =>
            client.BaseAddress = new Uri(server.Url!));
        ctx.Services.AddHttpClient(HttpClients.ApiAuthorized, client =>
            client.BaseAddress = new Uri(server.Url!));
        ctx.Services.AddScoped<IAssetService, AssetService>();

        var cut = ctx.Render<Download>(p =>
            p.Add(c => c.Id, assetId.ToString()));
        cut.WaitForState(
            () => !cut.FindAll(".spinner-border").Any(),
            TimeSpan.FromSeconds(5));

        cut.Find(".fw-semibold.text-truncate").TextContent.ShouldBe("test-file.txt");
        cut.Find(".text-secondary.small").TextContent.ShouldBe("text/plain");

        var downloadLink = cut.Find("a.btn-primary");
        downloadLink.GetAttribute("href")!.ShouldContain($"/assets/{assetId}/content");
        downloadLink.GetAttribute("download")!.ShouldBe("test-file.txt");
    }
}