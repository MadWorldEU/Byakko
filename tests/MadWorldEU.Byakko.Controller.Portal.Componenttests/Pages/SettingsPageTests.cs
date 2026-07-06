using MadWorldEU.Byakko.Accounts;

namespace MadWorldEU.Byakko.Pages;

/// <summary>Component tests for the Settings page.</summary>
public sealed class SettingsPageTests
{
    private static void RegisterServices(BunitContext ctx, string serverUrl)
    {
        ctx.Services.AddLocalization();
        ctx.Services.AddScoped<IErrorTranslator, ErrorTranslator>();
        ctx.Services.AddHttpClient(HttpClients.ApiAnonymous, client => client.BaseAddress = new Uri(serverUrl));
        ctx.Services.AddHttpClient(HttpClients.ApiAuthorized, client => client.BaseAddress = new Uri(serverUrl));
        ctx.Services.AddScoped<IAccountService, AccountService>();
    }

    private static GetMyAccountResponse MakeAccount(string status = "Active") => new()
    {
        UserId = Guid.NewGuid(),
        Status = status
    };

    private static void StubGetAccount(WireMockServer server, GetMyAccountResponse account) =>
        server
            .Given(Request.Create().WithPath("/accounts/me").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(account));

    private static void StubGetAccountNotFound(WireMockServer server, GetMyAccountResponse accountAfterCreation)
    {
        server
            .Given(Request.Create().WithPath("/accounts/me").UsingGet())
            .InScenario("account-init")
            .WillSetStateTo("created")
            .RespondWith(Response.Create()
                .WithStatusCode(404)
                .WithBodyAsJson(new { Code = "Account.NotFound", StatusCode = 404, Description = "Not found." }));

        server
            .Given(Request.Create().WithPath("/accounts/me").UsingGet())
            .InScenario("account-init")
            .WhenStateIs("created")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(accountAfterCreation));
    }

    private static void StubGetAccountFailed(WireMockServer server) =>
        server
            .Given(Request.Create().WithPath("/accounts/me").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(500)
                .WithBodyAsJson(new { Code = "Account.QueryFailed", StatusCode = 500, Description = "Query failed." }));

    private static void StubCreateAccount(WireMockServer server) =>
        server
            .Given(Request.Create().WithPath("/accounts/me").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(201)
                .WithBodyAsJson(new { UserId = Guid.NewGuid() }));

    private static void StubRequestDeletionSuccess(WireMockServer server) =>
        server
            .Given(Request.Create().WithPath("/accounts/me/deletion-request").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new { UserId = Guid.NewGuid() }));

    private static void StubRequestDeletionFailed(WireMockServer server, int statusCode, string code) =>
        server
            .Given(Request.Create().WithPath("/accounts/me/deletion-request").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(statusCode)
                .WithBodyAsJson(new { Code = code, StatusCode = statusCode, Description = "Failed." }));

    [Test]
    public void OnInitializedAsync_WhenAccountExists_ShouldShowActiveBadge()
    {
        using var server = WireMockServer.Start();
        StubGetAccount(server, MakeAccount());

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<Settings>();
        cut.WaitForState(() => cut.FindAll(".badge.text-bg-success").Any(), TimeSpan.FromSeconds(5));

        cut.Find(".badge.text-bg-success").TextContent.ShouldBe("Active");
    }

    [Test]
    public void OnInitializedAsync_WhenAccountNotFound_ShouldCreateAccountAndShowActiveBadge()
    {
        using var server = WireMockServer.Start();
        StubGetAccountNotFound(server, MakeAccount());
        StubCreateAccount(server);

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<Settings>();
        cut.WaitForState(() => cut.FindAll(".badge.text-bg-success").Any(), TimeSpan.FromSeconds(5));

        cut.Find(".badge.text-bg-success").TextContent.ShouldBe("Active");
    }

    [Test]
    public void OnInitializedAsync_WhenAccountHasDeletionRequested_ShouldShowDeletionRequestedBadge()
    {
        using var server = WireMockServer.Start();
        StubGetAccount(server, MakeAccount(status: "DeletionRequested"));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<Settings>();
        cut.WaitForState(() => cut.FindAll(".badge.text-bg-warning").Any(), TimeSpan.FromSeconds(5));

        cut.Find(".badge.text-bg-warning").TextContent.ShouldBe("Deletion requested");
        cut.FindAll(".alert-warning").ShouldNotBeEmpty();
    }

    [Test]
    public async Task OnInitializedAsync_WhenApiFails_ShouldShowErrorMessage()
    {
        using var server = WireMockServer.Start();
        StubGetAccountFailed(server);

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<Settings>();
        await cut.WaitForStateAsync(() => cut.FindAll(".alert-danger").Any(), TimeSpan.FromSeconds(5));

        cut.FindAll(".alert-danger").ShouldNotBeEmpty();
    }

    [Test]
    public void RequestDeletion_WhenButtonClicked_ShouldShowConfirmPrompt()
    {
        using var server = WireMockServer.Start();
        StubGetAccount(server, MakeAccount());

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<Settings>();
        cut.WaitForState(() => cut.FindAll(".btn-outline-danger").Any(), TimeSpan.FromSeconds(5));

        cut.Find(".btn-outline-danger").Click();

        cut.FindAll(".text-warning").ShouldNotBeEmpty();
        cut.FindAll(".btn-danger").ShouldNotBeEmpty();
        cut.FindAll(".btn-outline-secondary").ShouldNotBeEmpty();
    }

    [Test]
    public void CancelDeletion_WhenCancelClicked_ShouldRestoreDeleteButton()
    {
        using var server = WireMockServer.Start();
        StubGetAccount(server, MakeAccount());

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<Settings>();
        cut.WaitForState(() => cut.FindAll(".btn-outline-danger").Any(), TimeSpan.FromSeconds(5));

        cut.Find(".btn-outline-danger").Click();
        cut.WaitForState(() => cut.FindAll(".btn-outline-secondary").Any(), TimeSpan.FromSeconds(5));

        cut.Find(".btn-outline-secondary").Click();

        cut.FindAll(".btn-outline-danger").ShouldNotBeEmpty();
        cut.FindAll(".text-warning").ShouldBeEmpty();
    }

    [Test]
    public async Task ConfirmDeletionAsync_WhenSucceeds_ShouldShowSuccessMessage()
    {
        using var server = WireMockServer.Start();
        StubGetAccount(server, MakeAccount());
        StubRequestDeletionSuccess(server);

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<Settings>();
        cut.WaitForState(() => cut.FindAll(".btn-outline-danger").Any(), TimeSpan.FromSeconds(5));

        cut.Find(".btn-outline-danger").Click();
        cut.WaitForState(() => cut.FindAll(".btn-danger").Any(), TimeSpan.FromSeconds(5));

        cut.Find(".btn-danger").Click();
        await cut.WaitForStateAsync(() => cut.FindAll(".alert-success").Any(), TimeSpan.FromSeconds(5));

        cut.FindAll(".alert-success").ShouldNotBeEmpty();
        cut.Find(".badge.text-bg-warning").TextContent.ShouldBe("Deletion requested");
    }

    [Test]
    public async Task ConfirmDeletionAsync_WhenApiFails_ShouldShowDeletionErrorMessage()
    {
        using var server = WireMockServer.Start();
        StubGetAccount(server, MakeAccount());
        StubRequestDeletionFailed(server, 400, "Account.UpdateFailed");

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<Settings>();
        cut.WaitForState(() => cut.FindAll(".btn-outline-danger").Any(), TimeSpan.FromSeconds(5));

        cut.Find(".btn-outline-danger").Click();
        cut.WaitForState(() => cut.FindAll(".btn-danger").Any(), TimeSpan.FromSeconds(5));

        cut.Find(".btn-danger").Click();
        await cut.WaitForStateAsync(() => cut.FindAll(".alert-danger").Any(), TimeSpan.FromSeconds(5));

        cut.FindAll(".alert-danger").ShouldNotBeEmpty();
        cut.FindAll(".btn-outline-danger").ShouldNotBeEmpty();
    }
}