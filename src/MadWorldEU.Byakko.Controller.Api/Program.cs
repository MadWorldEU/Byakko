using MadWorldEU.Byakko.Endpoints.Development;
using MadWorldEU.Byakko.Endpoints.Storage;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();

app.AddFilesEndpoints();
app.AddTestsEndpoints();

app.Run();

public partial class Program { }