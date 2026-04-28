namespace MadWorldEU.Byakko.Endpoints.Development;

public static class TestsEndpoints
{
    public static void AddTestsEndpoints(this WebApplication app)
    {
        var testEndpoints = app.MapGroup("/tests")
            .WithGroupName("Tests");
        
        testEndpoints.MapGet("/ping", () => "pong");
    }
}