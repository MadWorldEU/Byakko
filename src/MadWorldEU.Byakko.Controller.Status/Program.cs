using MadWorldEU.Byakko.Application.Healths;
using MadWorldEU.Byakko.Configurations;
using MadWorldEU.Byakko.Extensions;
using OpenTelemetry.Resources;

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
    .ConfigureResource(resource =>
    {
        resource.AddAttributes([new KeyValuePair<string, object>("log_source", "application")]);
        resource.AddAttributes([new KeyValuePair<string, object>("service_name", "status")]);
    })
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
        .AddOtlpExporter(), options =>
    {
        options.IncludeFormattedMessage = true;
        options.IncludeScopes = true;
    });

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

await app.RunAsync();

public sealed partial class Program
{
    private Program() {}
}