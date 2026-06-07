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
        cut.Find("tbody tr td").TextContent.ShouldBe("document.pdf");
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

        cut.Find("tbody tr td").TextContent.ShouldBe("page2-file.png");
    }

    private static void RegisterServices(BunitContext ctx, string serverUrl)
    {
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

    private static AssetMetadataResponse MakeAsset(string name) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        ContentType = "application/pdf",
        UserId = Guid.NewGuid(),
        CreatedAt = DateTimeOffset.Parse("2026-05-01T10:00:00Z"),
        UpdatedAt = DateTimeOffset.Parse("2026-05-01T10:00:00Z"),
        ExpiresAt = DateTimeOffset.Parse("2026-06-01T10:00:00Z"),
        IsDeleted = false,
        Size = 1024
    };
}