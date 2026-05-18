using MadWorldEU.Byakko.Configurations;
using MadWorldEU.Byakko.Endpoints.Development;
using MadWorldEU.Byakko.Endpoints.Storages;
using MadWorldEU.Byakko.Extensions;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);
    
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.Services.AddDefaultCors(builder.Configuration);

builder.Services.AddHealthChecks();
builder.Services.AddBuildingBlocks();
builder.Services.AddApplication();
builder.Services.AddObjectStorage();    
builder.Services.AddPostgresql(builder.Configuration);

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
        .AddOtlpExporter());

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapOpenApi();
app.MapScalarApiReference();

app.MapHealthChecks("/health")
    .DisableRateLimiting();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

if (app.Configuration.GetValue("RateLimiting:Enabled", true))
{
    app.UseRateLimiter();   
}

app.AddAssetsEndpoints();
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