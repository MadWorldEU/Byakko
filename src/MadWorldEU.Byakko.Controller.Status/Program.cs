using MadWorldEU.Byakko.Application.Healths;
using MadWorldEU.Byakko.Configurations;
using MadWorldEU.Byakko.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddBuildingBlocks();
builder.Services.AddObjectStorage(builder.Configuration);
builder.Services.AddPostgresql(builder.Configuration);

builder.Services.AddHttpClient();
builder.Services.Configure<HealthCheckSettings>(builder.Configuration.GetSection("HealthChecks"));
builder.Services.AddScoped<GetHealthServicesUseCase>();
builder.Services.AddStatusRateLimiter(builder.Configuration);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter())
    .WithLogging(logging => logging
        .AddOtlpExporter());

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddPostgresql(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapHealthChecks("/health")
    .DisableRateLimiting();

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

if (app.Configuration.GetValue("RateLimiting:Enabled", true))
{
    app.UseRateLimiter();
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<MadWorldEU.Byakko.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();

public sealed partial class Program
{
    private Program() {}
}