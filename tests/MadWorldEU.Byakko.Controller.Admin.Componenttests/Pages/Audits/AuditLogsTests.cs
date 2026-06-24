namespace MadWorldEU.Byakko.Pages.Audits;

/// <summary>Component tests for the AuditLogs page.</summary>
public sealed class AuditLogsTests
{
    [Test]
    public void OnInitializedAsync_WhenLogsExist_ShouldShowLogRows()
    {
        var entityId = Guid.NewGuid();
        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath($"/audits/{entityId}").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(MakeResponse([MakeLog("Asset", "Created"), MakeLog("Asset", "Uploaded")])));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<AuditLogs>(p => p.Add(x => x.EntityId, entityId));
        cut.WaitForState(() => !cut.FindAll(".spinner-border").Any(), TimeSpan.FromSeconds(5));

        cut.FindAll("tbody tr").Count.ShouldBe(2);
    }

    [Test]
    public void OnInitializedAsync_WhenLogsExist_ShouldShowBadgesAndDetails()
    {
        var entityId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath($"/audits/{entityId}").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(MakeResponse([MakeLog("Asset", "Created", "171.129.229.213", userId, DateTimeOffset.Parse("2026-05-01T10:00:00Z"))])));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<AuditLogs>(p => p.Add(x => x.EntityId, entityId));
        cut.WaitForState(() => !cut.FindAll(".spinner-border").Any(), TimeSpan.FromSeconds(5));

        cut.Find("span.badge.bg-primary").TextContent.ShouldBe("Asset");
        cut.Find("span.badge.bg-secondary").TextContent.ShouldBe("Created");
        cut.FindAll("td.font-monospace")[0].TextContent.ShouldBe("171.129.229.213");
        cut.FindAll("td.font-monospace")[1].TextContent.ShouldBe(userId.ToString());
        cut.Find("td.small:not(.font-monospace)").TextContent.ShouldBe("2026-05-01 10:00:00");
    }

    [Test]
    public void OnInitializedAsync_WhenNoLogsExist_ShouldShowEmptyMessage()
    {
        var entityId = Guid.NewGuid();
        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath($"/audits/{entityId}").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(MakeResponse([])));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<AuditLogs>(p => p.Add(x => x.EntityId, entityId));
        cut.WaitForState(() => !cut.FindAll(".spinner-border").Any(), TimeSpan.FromSeconds(5));

        cut.Find("td.text-center.text-secondary").TextContent.ShouldBe("No audit log entries found.");
    }

    [Test]
    public void OnInitializedAsync_WhenApiFails_ShouldShowErrorAlert()
    {
        var entityId = Guid.NewGuid();
        using var server = WireMockServer.Start();
        server
            .Given(Request.Create().WithPath($"/audits/{entityId}").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(500));

        using var ctx = new BunitContext();
        RegisterServices(ctx, server.Url!);

        var cut = ctx.Render<AuditLogs>(p => p.Add(x => x.EntityId, entityId));
        cut.WaitForState(() => cut.FindAll(".alert-danger").Any(), TimeSpan.FromSeconds(5));

        cut.FindAll(".alert-danger").ShouldNotBeEmpty();
    }

    private static void RegisterServices(BunitContext ctx, string serverUrl)
    {
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddLocalization();
        ctx.Services.AddScoped<IErrorTranslator, ErrorTranslator>();
        ctx.Services.AddHttpClient(HttpClients.ApiAnonymous, client =>
            client.BaseAddress = new Uri(serverUrl));
        ctx.Services.AddHttpClient(HttpClients.ApiAuthorized, client =>
            client.BaseAddress = new Uri(serverUrl));
        ctx.Services.AddScoped<IAuditService, AuditService>();
    }

    private static GetAuditLogsResponse MakeResponse(IReadOnlyList<AuditLogResponse> logs) => new()
    {
        Logs = logs
    };

    private static AuditLogResponse MakeLog(
        string entityType,
        string action,
        string ipAddress = "1.2.3.4",
        Guid? occurredByUserId = null,
        DateTimeOffset? occurredAt = null) => new()
    {
        Id = Guid.NewGuid(),
        EntityType = entityType,
        Action = action,
        IpAddress = ipAddress,
        OccurredAt = occurredAt ?? DateTimeOffset.Parse("2026-05-01T10:00:00Z"),
        OccurredByUserId = occurredByUserId ?? Guid.NewGuid()
    };
}