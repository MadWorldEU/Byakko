namespace MadWorldEU.Byakko.Pages.Storages;

/// <summary>Component tests for the AssetsOverview page.</summary>
public sealed class AssetsOverviewTests
{
    [Test]
    public void OnInitializedAsync_WhenAssetsExist_ShouldShowAssetRows()
    {
        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath("/assets").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(MakeResponse([MakeAsset("document.pdf"), MakeAsset("report.xlsx")])));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<AssetsOverview>();
        cut.WaitForState(() => !cut.FindAll(".spinner-border").Any(), TimeSpan.FromSeconds(5));

        cut.FindAll("tbody tr").Count.ShouldBe(2);
        cut.Find("tbody tr td").TextContent.Trim().ShouldBe("document.pdf");
    }

    [Test]
    public void OnInitializedAsync_WhenNoAssetsExist_ShouldShowEmptyMessage()
    {
        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath("/assets").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(MakeResponse([])));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<AssetsOverview>();
        cut.WaitForState(() => !cut.FindAll(".spinner-border").Any(), TimeSpan.FromSeconds(5));

        cut.Find("td.text-center.text-secondary").TextContent.ShouldBe("No assets found.");
    }

    [Test]
    public void OnInitializedAsync_WhenApiFails_ShouldShowErrorAlert()
    {
        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath("/assets").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(500));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<AssetsOverview>();
        cut.WaitForState(() => cut.FindAll(".alert-danger").Any(), TimeSpan.FromSeconds(5));

        cut.FindAll(".alert-danger").ShouldNotBeEmpty();
    }

    [Test]
    public void OnInitializedAsync_WhenOnFirstPage_ShouldDisablePreviousButton()
    {
        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath("/assets").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(MakeResponse([], hasNextPage: true)));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<AssetsOverview>();
        cut.WaitForState(() => !cut.FindAll(".spinner-border").Any(), TimeSpan.FromSeconds(5));

        cut.FindAll("button.btn-outline-secondary")[0].HasAttribute("disabled").ShouldBeTrue();
    }

    [Test]
    public void OnInitializedAsync_WhenHasNoNextPage_ShouldDisableNextButton()
    {
        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath("/assets").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(MakeResponse([], hasNextPage: false)));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<AssetsOverview>();
        cut.WaitForState(() => !cut.FindAll(".spinner-border").Any(), TimeSpan.FromSeconds(5));

        cut.FindAll("button.btn-outline-secondary")[1].HasAttribute("disabled").ShouldBeTrue();
    }

    [Test]
    public void NextPageAsync_WhenHasNextPage_ShouldLoadNextPage()
    {
        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath("/assets").WithParam("page", "1").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(MakeResponse([MakeAsset("page1-file.pdf")], page: 1, hasNextPage: true)));
        server
            .Given(Request.Create().WithPath("/assets").WithParam("page", "2").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(MakeResponse([MakeAsset("page2-file.png")], page: 2, hasNextPage: false)));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<AssetsOverview>();
        cut.WaitForState(() => !cut.FindAll(".spinner-border").Any(), TimeSpan.FromSeconds(5));

        cut.FindAll("button.btn-outline-secondary")[1].Click();
        cut.WaitForState(
            () => cut.FindAll("td").Any(td => td.TextContent.Contains("page2-file.png")),
            TimeSpan.FromSeconds(5));

        cut.Find("tbody tr td").TextContent.Trim().ShouldBe("page2-file.png");
    }

    [Test]
    public void RequestDelete_WhenAssetIsNotDeleted_ShouldShowDeleteButton()
    {
        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath("/assets").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(MakeResponse([MakeAsset("document.pdf")])));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<AssetsOverview>();
        cut.WaitForState(() => !cut.FindAll(".spinner-border").Any(), TimeSpan.FromSeconds(5));

        cut.FindAll("button.btn-outline-danger").Count.ShouldBe(1);
    }

    [Test]
    public void RequestDelete_WhenAssetIsDeleted_ShouldNotShowDeleteButton()
    {
        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath("/assets").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(MakeResponse([MakeAsset("document.pdf", isDeleted: true)])));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<AssetsOverview>();
        cut.WaitForState(() => !cut.FindAll(".spinner-border").Any(), TimeSpan.FromSeconds(5));

        cut.FindAll("button.btn-outline-danger").ShouldBeEmpty();
    }

    [Test]
    public void RequestDelete_WhenDeleteButtonClicked_ShouldShowConfirmPrompt()
    {
        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath("/assets").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(MakeResponse([MakeAsset("document.pdf")])));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<AssetsOverview>();
        cut.WaitForState(() => !cut.FindAll(".spinner-border").Any(), TimeSpan.FromSeconds(5));

        cut.Find("button.btn-outline-danger").Click();

        cut.FindAll("button.btn-danger").ShouldNotBeEmpty();
        cut.FindAll(".text-warning").ShouldNotBeEmpty();
    }

    [Test]
    public void CancelDelete_WhenCancelClicked_ShouldRestoreDeleteButton()
    {
        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath("/assets").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(MakeResponse([MakeAsset("document.pdf")])));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<AssetsOverview>();
        cut.WaitForState(() => !cut.FindAll(".spinner-border").Any(), TimeSpan.FromSeconds(5));

        cut.Find("button.btn-outline-danger").Click();
        cut.Find("td button.btn-outline-secondary").Click();

        cut.FindAll("button.btn-danger").ShouldBeEmpty();
        cut.FindAll("button.btn-outline-danger").Count.ShouldBe(1);
    }

    [Test]
    public async Task ConfirmDeleteAsync_WhenConfirmed_ShouldCallDeleteApiAndReload()
    {
        var assetId = Guid.NewGuid();
        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath("/assets").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(MakeResponse([MakeAsset("document.pdf", id: assetId)])));
        server
            .Given(Request.Create().WithPath($"/assets/{assetId}/content").UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<AssetsOverview>();
        cut.WaitForState(() => !cut.FindAll(".spinner-border").Any(), TimeSpan.FromSeconds(5));

        cut.Find("button.btn-outline-danger").Click();
        cut.Find("button.btn-danger").Click();
        await cut.WaitForStateAsync(() => !cut.FindAll("button.btn-danger").Any(), TimeSpan.FromSeconds(5));

        var deleteRequests = server.LogEntries
            .Count(e => e.RequestMessage?.Path == $"/assets/{assetId}/content"
                        && e.RequestMessage?.Method == "DELETE");
        deleteRequests.ShouldBe(1);
    }

    [Test]
    public async Task OnSearchKeyDownAsync_WhenEnterPressedWithValidGuid_ShouldReloadWithSearchParams()
    {
        var searchGuid = Guid.NewGuid();
        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath("/assets").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(MakeResponse([MakeAsset("initial.pdf")])));
        server
            .Given(Request.Create()
                .WithPath("/assets")
                .WithParam("assetId", searchGuid.ToString())
                .WithParam("userId", searchGuid.ToString())
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(MakeResponse([MakeAsset("filtered.pdf")])));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<AssetsOverview>();
        cut.WaitForState(() => !cut.FindAll(".spinner-border").Any(), TimeSpan.FromSeconds(5));

        var input = cut.Find("input[type=search]");
        input.Input(searchGuid.ToString());
        await input.TriggerEventAsync("onkeydown", new KeyboardEventArgs { Key = "Enter" });
        await cut.WaitForStateAsync(
            () => cut.FindAll("td").Any(td => td.TextContent.Contains("filtered.pdf")),
            TimeSpan.FromSeconds(5));

        cut.Find("tbody tr td").TextContent.Trim().ShouldBe("filtered.pdf");
    }

    [Test]
    public async Task OnSearchKeyDownAsync_WhenEnterPressedWithInvalidInput_ShouldReloadWithoutSearchParams()
    {
        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath("/assets").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(MakeResponse([MakeAsset("document.pdf")])));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<AssetsOverview>();
        cut.WaitForState(() => !cut.FindAll(".spinner-border").Any(), TimeSpan.FromSeconds(5));

        var input = cut.Find("input[type=search]");
        input.Input("not-a-guid");
        await input.TriggerEventAsync("onkeydown", new KeyboardEventArgs { Key = "Enter" });
        await cut.WaitForStateAsync(
            () => server.LogEntries.Count(e => e.RequestMessage?.Path == "/assets") >= 2,
            TimeSpan.FromSeconds(5));

        server.LogEntries
            .Where(e => e.RequestMessage?.Path == "/assets")
            .All(e => e.RequestMessage?.Query?.ContainsKey("assetId") != true)
            .ShouldBeTrue();
    }

    [Test]
    public async Task OnSearchKeyDownAsync_WhenOtherKeyPressed_ShouldNotReload()
    {
        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath("/assets").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(MakeResponse([MakeAsset("document.pdf")])));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<AssetsOverview>();
        cut.WaitForState(() => !cut.FindAll(".spinner-border").Any(), TimeSpan.FromSeconds(5));

        await cut.Find("input[type=search]")
            .TriggerEventAsync("onkeydown", new KeyboardEventArgs { Key = "a" });

        server.LogEntries
            .Count(e => e.RequestMessage?.Path == "/assets" && e.RequestMessage?.Method == "GET")
            .ShouldBe(1);
    }

    private static void RegisterServices(BunitContext ctx, string serverUrl)
    {
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddHttpClient(HttpClients.ApiAnonymous, client =>
            client.BaseAddress = new Uri(serverUrl));
        ctx.Services.AddHttpClient(HttpClients.ApiAuthorized, client =>
            client.BaseAddress = new Uri(serverUrl));
        ctx.Services.AddScoped<IAssetService, AssetService>();
    }

    private static GetAssetsMetadataResponse MakeResponse(
        IReadOnlyList<AssetMetadataResponse> assets,
        int page = 1,
        bool hasNextPage = false) => new()
    {
        Assets = assets,
        Page = page,
        PageSize = 20,
        TotalCount = assets.Count,
        HasNextPage = hasNextPage
    };

    private static AssetMetadataResponse MakeAsset(string name, bool isDeleted = false, Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Name = name,
        ContentType = "application/pdf",
        UserId = Guid.NewGuid(),
        CreatedAt = DateTimeOffset.Parse("2026-05-01T10:00:00Z"),
        UpdatedAt = DateTimeOffset.Parse("2026-05-01T10:00:00Z"),
        ExpiresAt = DateTimeOffset.Parse("2026-06-01T10:00:00Z"),
        IsDeleted = isDeleted,
        Size = 1024
    };
}