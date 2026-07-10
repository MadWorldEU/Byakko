namespace MadWorldEU.Byakko.Pages.HostServices;

/// <summary>Component tests for the ManualTriggers host services page.</summary>
public sealed class ManualTriggersTests
{
    private static void RegisterServices(BunitContext ctx, string serverUrl)
    {
        ctx.Services.AddLogging();
        ctx.Services.AddLocalization();
        ctx.Services.AddScoped<IErrorTranslator, ErrorTranslator>();
        ctx.Services.AddHttpClient(HttpClients.ApiAnonymous, client => client.BaseAddress = new Uri(serverUrl));
        ctx.Services.AddHttpClient(HttpClients.ApiAuthorized, client => client.BaseAddress = new Uri(serverUrl));
    }

    private static void StubTrigger(WireMockServer server, string path, int statusCode = 200) =>
        server
            .Given(Request.Create().WithPath(path).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(statusCode));

    [Test]
    public async Task TriggerContentCleanupAsync_WhenApiSucceeds_ShouldShowSuccessAlert()
    {
        using var server = WireMockServer.Start();
        StubTrigger(server, "/host-services/manual-triggers/clean-up/assets-content");

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<ManualTriggers>();
        cut.FindAll(".btn-warning")[0].Click();

        await cut.WaitForStateAsync(() => cut.FindAll(".alert-success").Any(), TimeSpan.FromSeconds(5));

        cut.Find(".alert-success").TextContent.Trim().ShouldBe("Content cleanup completed successfully.");
    }

    [Test]
    public async Task TriggerContentCleanupAsync_WhenApiFails_ShouldShowErrorAlert()
    {
        using var server = WireMockServer.Start();
        StubTrigger(server, "/host-services/manual-triggers/clean-up/assets-content", 500);

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<ManualTriggers>();
        cut.FindAll(".btn-warning")[0].Click();

        await cut.WaitForStateAsync(() => cut.FindAll(".alert-danger").Any(), TimeSpan.FromSeconds(5));

        cut.Find(".alert-danger").TextContent.Trim().ShouldContain("Cleanup failed");
    }

    [Test]
    public async Task TriggerMetadataCleanupAsync_WhenApiSucceeds_ShouldShowSuccessAlert()
    {
        using var server = WireMockServer.Start();
        StubTrigger(server, "/host-services/manual-triggers/clean-up/assets-metadata");

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<ManualTriggers>();
        cut.FindAll(".btn-warning")[1].Click();

        await cut.WaitForStateAsync(() => cut.FindAll(".alert-success").Any(), TimeSpan.FromSeconds(5));

        cut.Find(".alert-success").TextContent.Trim().ShouldBe("Metadata cleanup completed successfully.");
    }

    [Test]
    public async Task TriggerMetadataCleanupAsync_WhenApiFails_ShouldShowErrorAlert()
    {
        using var server = WireMockServer.Start();
        StubTrigger(server, "/host-services/manual-triggers/clean-up/assets-metadata", 500);

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<ManualTriggers>();
        cut.FindAll(".btn-warning")[1].Click();

        await cut.WaitForStateAsync(() => cut.FindAll(".alert-danger").Any(), TimeSpan.FromSeconds(5));

        cut.Find(".alert-danger").TextContent.Trim().ShouldContain("Cleanup failed");
    }

    [Test]
    public async Task TriggerAccountsCleanupAsync_WhenApiSucceeds_ShouldShowSuccessAlert()
    {
        using var server = WireMockServer.Start();
        StubTrigger(server, "/host-services/manual-triggers/clean-up/accounts");

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<ManualTriggers>();
        cut.FindAll(".btn-warning")[2].Click();

        await cut.WaitForStateAsync(() => cut.FindAll(".alert-success").Any(), TimeSpan.FromSeconds(5));

        cut.Find(".alert-success").TextContent.Trim().ShouldBe("Accounts cleanup completed successfully.");
    }

    [Test]
    public async Task TriggerAccountsCleanupAsync_WhenApiFails_ShouldShowErrorAlert()
    {
        using var server = WireMockServer.Start();
        StubTrigger(server, "/host-services/manual-triggers/clean-up/accounts", 500);

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<ManualTriggers>();
        cut.FindAll(".btn-warning")[2].Click();

        await cut.WaitForStateAsync(() => cut.FindAll(".alert-danger").Any(), TimeSpan.FromSeconds(5));

        cut.Find(".alert-danger").TextContent.Trim().ShouldContain("Cleanup failed");
    }
}