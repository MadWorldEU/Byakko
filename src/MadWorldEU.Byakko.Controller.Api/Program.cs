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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

builder.Services.AddHealthChecks();
builder.Services.AddBuildingBlocks();
builder.Services.AddApplication();
builder.Services.AddObjectStorage();
builder.Services.AddPostgresql(builder.Configuration);

builder.AddDefaultAuthentication();
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

app.MapHealthChecks("/health");

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.AddAssetsEndpoints();
app.AddTestsEndpoints();

if (app.Environment.IsDevelopment())
{
    app.AddDebugEndpoints();
}

app.Run();

[UsedImplicitly]
public sealed partial class Program { }