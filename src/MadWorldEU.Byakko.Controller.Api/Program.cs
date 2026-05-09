using MadWorldEU.Byakko.Endpoints.Development;
using MadWorldEU.Byakko.Endpoints.Storages;
using MadWorldEU.Byakko.Extensions;

var builder = WebApplication.CreateBuilder(args);
    
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddBuildingBlocks();
builder.Services.AddApplication();
builder.Services.AddPostgresqlInfrastructure(builder.Configuration);

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

app.MapOpenApi();
app.MapScalarApiReference();

app.MapHealthChecks("/health");

app.AddAssetsEndpoints();
app.AddTestsEndpoints();

if (app.Environment.IsDevelopment())
{
    app.AddDebugEndpoints();   
}

app.Run();

[UsedImplicitly]
public sealed partial class Program { }