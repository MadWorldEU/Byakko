using NodaTime;

namespace MadWorldEU.Byakko.Pages.Storage;

/// <summary>Component tests for the My Assets page.</summary>
public sealed class MyAssetsPageTests
{
    private static void RegisterServices(BunitContext ctx, string serverUrl)
    {
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddLocalization();
        ctx.Services.AddSingleton<IClock>(SystemClock.Instance);
        ctx.Services.AddScoped<IErrorTranslator, ErrorTranslator>();
        ctx.Services.AddHttpClient(HttpClients.ApiAnonymous, client => client.BaseAddress = new Uri(serverUrl));
        ctx.Services.AddHttpClient(HttpClients.ApiAuthorized, client => client.BaseAddress = new Uri(serverUrl));
        ctx.Services.AddScoped<IAssetService, AssetService>();
    }

    private static void StubLimits(WireMockServer server) =>
        server
            .Given(Request.Create().WithPath("/assets/limits").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new GetUserUploadLimitsResponse { MaxFiles = 10, MaxUploadSizeInBytes = 1073741824, ActiveFiles = 1 }));

    private static void StubAssets(WireMockServer server, params AssetMetadataResponse[] assets) =>
        server
            .Given(Request.Create().WithPath("/assets/me").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new GetAssetsMetadataResponse
                {
                    Assets = [..assets],
                    Page = 1,
                    PageSize = 20,
                    TotalCount = assets.Length,
                    HasNextPage = false
                }));

    private static AssetMetadataResponse MakeAsset(string name, bool isDeleted = false, Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Name = name,
        ContentType = "text/plain",
        UserId = Guid.NewGuid(),
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow,
        ExpiresAt = DateTimeOffset.UtcNow.AddDays(30),
        IsDeleted = isDeleted,
        Size = 1024
    };

    [Test]
    public void OnInitializedAsync_WhenAssetsExist_ShouldShowAssetsTable()
    {
        var assetId = Guid.NewGuid();

        using var server = WireMockServer.Start();
        StubLimits(server);
        StubAssets(server, MakeAsset("test-file.txt", id: assetId));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<MyAssets>();
        cut.WaitForState(() => cut.FindAll(".list-group-item").Any(), TimeSpan.FromSeconds(5));

        cut.Find(".list-group-item .fw-medium").TextContent.ShouldBe("test-file.txt");
        cut.Find(".badge.text-bg-success").TextContent.ShouldBe("Active");
        cut.Find(".list-group-item a.btn").GetAttribute("href")!.ShouldContain($"storage/download/{assetId}");
    }

    [Test]
    public void OnInitializedAsync_WhenNoAssetsExist_ShouldShowEmptyState()
    {
        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath("/assets/limits").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new GetUserUploadLimitsResponse { MaxFiles = 10, MaxUploadSizeInBytes = 1073741824, ActiveFiles = 0 }));
        StubAssets(server);

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<MyAssets>();
        cut.WaitForState(() => cut.FindAll(".alert-info").Count >= 2, TimeSpan.FromSeconds(5));

        cut.FindAll(".alert-info")[1].TextContent.Trim().ShouldContain("You have not uploaded any files yet.");
    }

    [Test]
    public void RequestDelete_WhenAssetIsActive_ShouldShowDeleteButton()
    {
        using var server = WireMockServer.Start();
        StubLimits(server);
        StubAssets(server, MakeAsset("document.pdf"));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<MyAssets>();
        cut.WaitForState(() => cut.FindAll(".list-group-item").Any(), TimeSpan.FromSeconds(5));

        cut.FindAll("button.btn-outline-danger").Count.ShouldBe(1);
    }

    [Test]
    public void RequestDelete_WhenDeleteButtonClicked_ShouldShowConfirmPrompt()
    {
        using var server = WireMockServer.Start();
        StubLimits(server);
        StubAssets(server, MakeAsset("document.pdf"));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<MyAssets>();
        cut.WaitForState(() => cut.FindAll(".list-group-item").Any(), TimeSpan.FromSeconds(5));

        cut.Find("button.btn-outline-danger").Click();
        cut.WaitForState(() => cut.FindAll("button.btn-danger").Any(), TimeSpan.FromSeconds(5));

        cut.FindAll("button.btn-danger").ShouldNotBeEmpty();
        cut.FindAll(".text-warning").ShouldNotBeEmpty();
    }

    [Test]
    public void CancelDelete_WhenCancelClicked_ShouldRestoreDeleteButton()
    {
        using var server = WireMockServer.Start();
        StubLimits(server);
        StubAssets(server, MakeAsset("document.pdf"));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<MyAssets>();
        cut.WaitForState(() => cut.FindAll(".list-group-item").Any(), TimeSpan.FromSeconds(5));

        cut.Find("button.btn-outline-danger").Click();
        cut.Find("button.btn-outline-secondary").Click();

        cut.FindAll("button.btn-danger").ShouldBeEmpty();
        cut.FindAll("button.btn-outline-danger").Count.ShouldBe(1);
    }

    [Test]
    public async Task ConfirmDeleteAsync_WhenConfirmed_ShouldCallDeleteApiAndReload()
    {
        var assetId = Guid.NewGuid();

        using var server = WireMockServer.Start();
        StubLimits(server);
        StubAssets(server, MakeAsset("document.pdf", id: assetId));
        server
            .Given(Request.Create().WithPath($"/assets/me/{assetId}/content").UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<MyAssets>();
        cut.WaitForState(() => cut.FindAll(".list-group-item").Any(), TimeSpan.FromSeconds(5));

        cut.Find("button.btn-outline-danger").Click();
        cut.Find("button.btn-danger").Click();
        await cut.WaitForStateAsync(() => !cut.FindAll("button.btn-danger").Any(), TimeSpan.FromSeconds(5));

        var deleteRequests = server.LogEntries
            .Count(e => e.RequestMessage?.Path == $"/assets/me/{assetId}/content"
                        && e.RequestMessage?.Method == "DELETE");
        deleteRequests.ShouldBe(1);
    }
}