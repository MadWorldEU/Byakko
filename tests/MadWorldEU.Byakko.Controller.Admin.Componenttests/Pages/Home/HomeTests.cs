namespace MadWorldEU.Byakko.Pages;

/// <summary>Component tests for the Home dashboard page.</summary>
public sealed class HomeTests
{
    [Test]
    public void OnInitializedAsync_WhenStatisticsLoaded_ShouldShowTotalFilesCount()
    {
        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath("/storage/statistics").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new { TotalFiles = 5, TotalBytes = 0L }));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<Home>();
        cut.WaitForState(
            () => cut.Find(".stat-card-primary .fw-bold.fs-2").TextContent.Trim() != "—",
            TimeSpan.FromSeconds(5));

        cut.Find(".stat-card-primary .fw-bold.fs-2").TextContent.Trim().ShouldBe("5");
    }

    [Test]
    public void OnInitializedAsync_WhenStatisticsLoaded_ShouldShowFormattedStorageUsed()
    {
        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath("/storage/statistics").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new { TotalFiles = 0, TotalBytes = 2048L }));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<Home>();
        cut.WaitForState(
            () => cut.Find(".stat-card-info .fw-bold.fs-2").TextContent.Trim() != "—",
            TimeSpan.FromSeconds(5));

        cut.Find(".stat-card-info .fw-bold.fs-2").TextContent.Trim().ShouldBe("2.0 KB");
    }

    private static void RegisterServices(BunitContext ctx, string serverUrl)
    {
        ctx.Services.AddLocalization();
        ctx.Services.AddScoped<IErrorTranslator, ErrorTranslator>();
        ctx.Services.AddHttpClient(HttpClients.ApiAnonymous, client =>
            client.BaseAddress = new Uri(serverUrl));
        ctx.Services.AddHttpClient(HttpClients.ApiAuthorized, client =>
            client.BaseAddress = new Uri(serverUrl));
        ctx.Services.AddScoped<IStorageService, StorageService>();
    }
}