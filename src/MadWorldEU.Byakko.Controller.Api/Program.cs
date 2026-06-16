using MadWorldEU.Byakko.Configurations;
using MadWorldEU.Byakko.Endpoints.Audits;
using MadWorldEU.Byakko.Endpoints.Development;
using MadWorldEU.Byakko.Endpoints.HostServices;
using MadWorldEU.Byakko.Endpoints.Storages;
using MadWorldEU.Byakko.Extensions;
using MadWorldEU.Byakko.HostedServices;
using Microsoft.AspNetCore.HttpOverrides;

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
        .AddOtlpExporter(), options => options.IncludeFormattedMessage = true);

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

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