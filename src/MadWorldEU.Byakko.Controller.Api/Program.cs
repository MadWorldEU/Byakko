using MadWorldEU.Byakko.Configurations;
using MadWorldEU.Byakko.Endpoints.Audits;
using MadWorldEU.Byakko.Endpoints.Correspondences;
using MadWorldEU.Byakko.Endpoints.Development;
using MadWorldEU.Byakko.Endpoints.HostServices;
using MadWorldEU.Byakko.Endpoints.Storages;
using MadWorldEU.Byakko.HostedServices;
using MadWorldEU.Byakko.Middlewares;
using Microsoft.AspNetCore.HttpOverrides;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);
    
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddDocumentTransformer<ServerUrlDocumentTransformer>();
});

builder.Services.AddDefaultCors(builder.Configuration);

builder.Services.AddHealthChecks();
builder.Services.AddBuildingBlocks();
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddMail(builder.Configuration);
builder.Services.AddObjectStorage(builder.Configuration);
builder.Services.AddPostgresql(builder.Configuration);
builder.Services.AddSecurity(builder.Configuration);

builder.Services.Configure<CleanupSettings>(options => 
    builder.Configuration.GetSection(CleanupSettings.Key).Bind(options));
builder.Services.AddHostedService<DeleteExpiredAssetsService>();
builder.Services.AddHostedService<DeleteExpiredAssetMetaDataService>();

builder.AddDefaultAuthentication();
builder.Services.AddApiRateLimiter(builder.Configuration);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource =>
    {
        resource.AddAttributes([new KeyValuePair<string, object>("log_source", "application")]);
    })
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddMeter(AssetMetrics.MeterName)
        .AddMeter(CorrespondenceMetrics.MeterName)
        .AddOtlpExporter())
    .WithLogging(logging => logging
        .AddOtlpExporter(), options =>
    {
        options.IncludeFormattedMessage = true;
        options.IncludeScopes = true;
    });

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseStaticFiles();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapHealthChecks("/health")
    .DisableRateLimiting();

app.UseCors();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseAuthentication();
app.UseAuthorization();

if (app.Configuration.GetValue("RateLimiting:Enabled", true))
{
    app.UseRateLimiter();   
}

app.AddAuditEndpoints();
app.AddAssetsEndpoints();
app.AddCorrespondenceEndpoints();
app.AddGeneralStorageEndpoints();
app.AddManualTriggersEndpoints();
app.AddTestsEndpoints();

if (app.Environment.IsDevelopment())
{
    app.AddDebugEndpoints();
}

await app.RunAsync();

public sealed partial class Program
{
    private Program() {}
}