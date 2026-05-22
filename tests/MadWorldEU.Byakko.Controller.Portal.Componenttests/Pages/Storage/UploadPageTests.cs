using TestContext = Bunit.TestContext;

namespace MadWorldEU.Byakko.Pages.Storage;

/// <summary>Component tests for the Upload page.</summary>
public sealed class UploadPageTests
{
    [Test]
    public void UploadAsync_WhenFileSelected_ShouldShowShareLink()
    {
        var assetId = Guid.NewGuid();

        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath("/assets").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new CreateAssetResponse { Id = assetId }));
        server
            .Given(Request.Create().WithPath($"/assets/{assetId}/content").UsingPut())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new UploadAssetContentResponse { Id = assetId }));

        using var ctx = new TestContext();
        ctx.Services.AddHttpClient(HttpClients.ApiAnonymous, client =>
            client.BaseAddress = new Uri(server.Url!));
        ctx.Services.AddHttpClient(HttpClients.ApiAuthorized, client =>
            client.BaseAddress = new Uri(server.Url!));
        ctx.Services.AddScoped<IAssetService, AssetService>();

        var cut = ctx.RenderComponent<Upload>();

        cut.FindComponent<InputFile>()
            .UploadFiles(InputFileContent.CreateFromText("Hello, World!", "test.txt", contentType: "text/plain"));

        cut.Find("button.btn-primary").Click();
        cut.WaitForState(
            () => cut.FindAll(".alert-success").Any(),
            TimeSpan.FromSeconds(5));

        cut.Find(".alert-success .fw-semibold").TextContent.ShouldBe("File uploaded successfully!");
        cut.Find(".form-control.font-monospace").GetAttribute("value")!
            .ShouldContain($"storage/download/{assetId}");
    }
}