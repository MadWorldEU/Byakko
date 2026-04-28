namespace MadWorldEU.Byakko.Endpoints.Development;

public static class TestsEndpoints
{
    public static void AddTestsEndpoints(this WebApplication app)
    {
        var testEndpoints = app.MapGroup("/tests")
            .WithTags("Tests");
        
        testEndpoints.MapGet("/ping", () => "pong")
            .WithName("Ping");       
    }
}