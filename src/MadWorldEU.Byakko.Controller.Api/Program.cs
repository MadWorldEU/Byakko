using MadWorldEU.Byakko.Endpoints.Development;
using MadWorldEU.Byakko.Endpoints.Storage;

var builder = WebApplication.CreateBuilder(args);
    
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddDbContext<ByakkoContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("byakko-db")));

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapHealthChecks("/health");

app.AddFilesEndpoints();
app.AddTestsEndpoints();

if (app.Environment.IsDevelopment())
{
    app.AddDebugEndpoints();   
}

app.Run();

[UsedImplicitly]
public partial class Program { }