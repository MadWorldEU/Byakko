using MadWorldEU.Byakko.Accounts;
using MadWorldEU.Byakko.Accounts.Summaries;

namespace MadWorldEU.Byakko.Pages.Accounts;

/// <summary>Component tests for the AccountDeletionRequests page.</summary>
public sealed class AccountDeletionRequestsTests
{
    private static void RegisterServices(BunitContext ctx, string serverUrl)
    {
        ctx.Services.AddLocalization();
        ctx.Services.AddScoped<IErrorTranslator, ErrorTranslator>();
        ctx.Services.AddHttpClient(HttpClients.ApiAnonymous, client => client.BaseAddress = new Uri(serverUrl));
        ctx.Services.AddHttpClient(HttpClients.ApiAuthorized, client => client.BaseAddress = new Uri(serverUrl));
        ctx.Services.AddScoped<IAccountService, AccountService>();
    }

    private static GetDeleteRequestedAccountsResponse MakeResponse(
        IReadOnlyList<AccountResponse> accounts,
        int page = 1,
        bool hasNextPage = false) => new()
    {
        Accounts = [..accounts],
        Page = page,
        PageSize = 20,
        TotalCount = accounts.Count,
        HasNextPage = hasNextPage
    };

    private static AccountResponse MakeAccount(Guid? userId = null) => new()
    {
        UserId = userId ?? Guid.NewGuid(),
        HasDeletionRequested = true
    };

    private static void StubAccounts(WireMockServer server, GetDeleteRequestedAccountsResponse response, int page = 1) =>
        server
            .Given(Request.Create().WithPath("/accounts").WithParam("page", page.ToString()).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(response));

    [Test]
    public void OnInitializedAsync_WhenAccountsExist_ShouldShowAccountRows()
    {
        using var server = WireMockServer.Start();
        StubAccounts(server, MakeResponse([MakeAccount(), MakeAccount()]));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<AccountDeletionRequests>();
        cut.WaitForState(() => !cut.FindAll(".spinner-border").Any(), TimeSpan.FromSeconds(5));

        cut.FindAll("tbody tr").Count.ShouldBe(2);
        cut.Find(".badge.text-bg-warning").TextContent.ShouldBe("Deletion requested");
    }

    [Test]
    public void OnInitializedAsync_WhenNoAccountsExist_ShouldShowEmptyMessage()
    {
        using var server = WireMockServer.Start();
        StubAccounts(server, MakeResponse([]));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<AccountDeletionRequests>();
        cut.WaitForState(() => !cut.FindAll(".spinner-border").Any(), TimeSpan.FromSeconds(5));

        cut.Find("td.text-center.text-secondary").TextContent.ShouldBe("No deletion requests found.");
    }

    [Test]
    public void OnInitializedAsync_WhenApiFails_ShouldShowErrorAlert()
    {
        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath("/accounts").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(500));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<AccountDeletionRequests>();
        cut.WaitForState(() => cut.FindAll(".alert-danger").Any(), TimeSpan.FromSeconds(5));

        cut.FindAll(".alert-danger").ShouldNotBeEmpty();
    }

    [Test]
    public void OnInitializedAsync_WhenOnFirstPage_ShouldDisablePreviousButton()
    {
        using var server = WireMockServer.Start();
        StubAccounts(server, MakeResponse([], hasNextPage: true));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<AccountDeletionRequests>();
        cut.WaitForState(() => !cut.FindAll(".spinner-border").Any(), TimeSpan.FromSeconds(5));

        cut.FindAll("button.btn-outline-secondary")[0].HasAttribute("disabled").ShouldBeTrue();
    }

    [Test]
    public void OnInitializedAsync_WhenHasNoNextPage_ShouldDisableNextButton()
    {
        using var server = WireMockServer.Start();
        StubAccounts(server, MakeResponse([], hasNextPage: false));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<AccountDeletionRequests>();
        cut.WaitForState(() => !cut.FindAll(".spinner-border").Any(), TimeSpan.FromSeconds(5));

        cut.FindAll("button.btn-outline-secondary")[1].HasAttribute("disabled").ShouldBeTrue();
    }

    [Test]
    public void NextPageAsync_WhenHasNextPage_ShouldLoadNextPage()
    {
        var accountPage2 = MakeAccount();
        using var server = WireMockServer.Start();
        StubAccounts(server, MakeResponse([MakeAccount()], page: 1, hasNextPage: true), page: 1);
        StubAccounts(server, MakeResponse([accountPage2], page: 2, hasNextPage: false), page: 2);

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<AccountDeletionRequests>();
        cut.WaitForState(() => !cut.FindAll(".spinner-border").Any(), TimeSpan.FromSeconds(5));

        cut.FindAll("button.btn-outline-secondary")[1].Click();
        cut.WaitForState(
            () => cut.FindAll("td").Any(td => td.TextContent.Contains(accountPage2.UserId.ToString())),
            TimeSpan.FromSeconds(5));

        cut.Find("tbody tr td").TextContent.Trim().ShouldBe(accountPage2.UserId.ToString());
    }
}