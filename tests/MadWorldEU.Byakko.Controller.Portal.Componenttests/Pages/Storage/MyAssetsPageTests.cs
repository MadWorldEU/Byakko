namespace MadWorldEU.Byakko.Pages.Storage;

/// <summary>Component tests for the My Assets page.</summary>
public sealed class MyAssetsPageTests
{
    [Test]
    public void OnInitializedAsync_WhenAssetsExist_ShouldShowAssetsTable()
    {
        var assetId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(30);

        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath("/assets/limits").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new GetUserUploadLimitsResponse { MaxFiles = 10, MaxUploadSizeInBytes = 1073741824, ActiveFiles = 1 }));
        server
            .Given(Request.Create().WithPath("/assets/me").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new GetAssetsMetadataResponse
                {
                    Assets =
                    [
                        new AssetMetadataResponse
                        {
                            Id = assetId,
                            Name = "test-file.txt",
                            ContentType = "text/plain",
                            UserId = Guid.NewGuid(),
                            CreatedAt = DateTimeOffset.UtcNow,
                            UpdatedAt = DateTimeOffset.UtcNow,
                            ExpiresAt = expiresAt,
                            IsDeleted = false,
                            Size = 1024
                        }
                    ],
                    Page = 1,
                    PageSize = 20,
                    TotalCount = 1,
                    HasNextPage = false
                }));

        using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddHttpClient(HttpClients.ApiAnonymous, client =>
            client.BaseAddress = new Uri(server.Url!));
        ctx.Services.AddHttpClient(HttpClients.ApiAuthorized, client =>
            client.BaseAddress = new Uri(server.Url!));
        ctx.Services.AddScoped<IAssetService, AssetService>();

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
        server
            .Given(Request.Create().WithPath("/assets/me").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new GetAssetsMetadataResponse
                {
                    Assets = [],
                    Page = 1,
                    PageSize = 20,
                    TotalCount = 0,
                    HasNextPage = false
                }));

        using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddHttpClient(HttpClients.ApiAnonymous, client =>
            client.BaseAddress = new Uri(server.Url!));
        ctx.Services.AddHttpClient(HttpClients.ApiAuthorized, client =>
            client.BaseAddress = new Uri(server.Url!));
        ctx.Services.AddScoped<IAssetService, AssetService>();

        var cut = ctx.Render<MyAssets>();
        cut.WaitForState(() => cut.FindAll(".alert-info").Count >= 2, TimeSpan.FromSeconds(5));

        cut.FindAll(".alert-info")[1].TextContent.Trim().ShouldContain("You have not uploaded any files yet.");
    }
}