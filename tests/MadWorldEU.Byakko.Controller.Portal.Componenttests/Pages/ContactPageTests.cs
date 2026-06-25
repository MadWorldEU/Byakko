using System.Security.Claims;
using MadWorldEU.Byakko.Correspondences;
using Microsoft.AspNetCore.Components.Authorization;

namespace MadWorldEU.Byakko.Pages;

/// <summary>Component tests for the Contact page.</summary>
public sealed class ContactPageTests
{
    private static void RegisterServices(BunitContext ctx, string serverUrl)
    {
        ctx.Services.AddLocalization();
        ctx.Services.AddScoped<IErrorTranslator, ErrorTranslator>();
        ctx.Services.AddHttpClient(HttpClients.ApiAnonymous, client => client.BaseAddress = new Uri(serverUrl));
        ctx.Services.AddHttpClient(HttpClients.ApiAuthorized, client => client.BaseAddress = new Uri(serverUrl));
        ctx.Services.AddScoped<ICorrespondenceService, CorrespondenceService>();
    }

    private static void SetAnonymousAuth(BunitContext ctx) =>
        ctx.Services.AddCascadingValue<Task<AuthenticationState>>(_ =>
            Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));

    private static void SetAuthenticatedAuth(BunitContext ctx, string email) =>
        ctx.Services.AddCascadingValue<Task<AuthenticationState>>(_ =>
            Task.FromResult(new AuthenticationState(new ClaimsPrincipal(
                new ClaimsIdentity([new Claim(ClaimTypes.Email, email)], "testauth")))));

    private static void StubFeedbackSuccess(WireMockServer server) =>
        server
            .Given(Request.Create().WithPath("/correspondences/feedback").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200));

    private static void StubFeedbackFailure(WireMockServer server, int statusCode, string code, string description) =>
        server
            .Given(Request.Create().WithPath("/correspondences/feedback").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(statusCode)
                .WithBodyAsJson(new { Code = code, StatusCode = statusCode, Description = description }));

    [Test]
    public void OnInitializedAsync_WhenAnonymous_ShouldShowEmptyEmailField()
    {
        using var server = WireMockServer.Start();
        using var ctx = new BunitContext();
        SetAnonymousAuth(ctx);
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<Contact>();

        cut.Find("input[type='email']").GetAttribute("value").ShouldBeNullOrEmpty();
    }

    [Test]
    public void OnInitializedAsync_WhenAuthenticated_ShouldPrefillEmailField()
    {
        using var server = WireMockServer.Start();
        using var ctx = new BunitContext();
        SetAuthenticatedAuth(ctx, "user@example.com");
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<Contact>();
        cut.WaitForState(() => cut.Find("input[type='email']").GetAttribute("value") == "user@example.com", TimeSpan.FromSeconds(5));

        cut.Find("input[type='email']").GetAttribute("value").ShouldBe("user@example.com");
    }

    [Test]
    public void SendAsync_WhenEmailIsEmpty_ShouldShowEmailRequiredError()
    {
        using var server = WireMockServer.Start();
        using var ctx = new BunitContext();
        SetAnonymousAuth(ctx);
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<Contact>();
        cut.Find("button.btn-primary").Click();

        cut.Find("input[type='email']").ClassList.ShouldContain("is-invalid");
        cut.FindAll(".invalid-feedback").ShouldNotBeEmpty();
    }

    [Test]
    public void SendAsync_WhenEmailIsInvalid_ShouldShowEmailInvalidError()
    {
        using var server = WireMockServer.Start();
        using var ctx = new BunitContext();
        SetAnonymousAuth(ctx);
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<Contact>();
        cut.Find("input[type='email']").Change("notanemail");
        cut.Find("button.btn-primary").Click();

        cut.Find("input[type='email']").ClassList.ShouldContain("is-invalid");
        cut.FindAll(".invalid-feedback").ShouldNotBeEmpty();
    }

    [Test]
    public void SendAsync_WhenMessageIsEmpty_ShouldShowMessageRequiredError()
    {
        using var server = WireMockServer.Start();
        using var ctx = new BunitContext();
        SetAnonymousAuth(ctx);
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<Contact>();
        cut.Find("input[type='email']").Change("user@example.com");
        cut.Find("button.btn-primary").Click();

        cut.Find("textarea").ClassList.ShouldContain("is-invalid");
        cut.FindAll(".invalid-feedback").ShouldNotBeEmpty();
    }

    [Test]
    public async Task SendAsync_WhenFeedbackSent_ShouldShowSuccessMessage()
    {
        using var server = WireMockServer.Start();
        StubFeedbackSuccess(server);

        using var ctx = new BunitContext();
        SetAnonymousAuth(ctx);
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<Contact>();
        cut.Find("input[type='email']").Change("user@example.com");
        cut.Find("textarea").Change("This is my feedback message.");
        cut.Find("button.btn-primary").Click();

        await cut.WaitForStateAsync(() => cut.FindAll(".alert-success").Any(), TimeSpan.FromSeconds(5));

        cut.FindAll(".alert-success").ShouldNotBeEmpty();
        cut.FindAll(".card").ShouldBeEmpty();
    }

    [Test]
    public async Task SendAsync_WhenApiFails_ShouldShowErrorMessage()
    {
        using var server = WireMockServer.Start();
        StubFeedbackFailure(server, 400, "Feedback.Failed", "Feedback could not be sent.");

        using var ctx = new BunitContext();
        SetAnonymousAuth(ctx);
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<Contact>();
        cut.Find("input[type='email']").Change("user@example.com");
        cut.Find("textarea").Change("This is my feedback message.");
        cut.Find("button.btn-primary").Click();

        await cut.WaitForStateAsync(() => cut.FindAll(".alert-danger").Any(), TimeSpan.FromSeconds(5));

        cut.FindAll(".alert-danger").ShouldNotBeEmpty();
    }
}