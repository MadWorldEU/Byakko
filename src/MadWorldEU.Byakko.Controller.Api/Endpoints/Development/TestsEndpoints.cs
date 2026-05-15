namespace MadWorldEU.Byakko.Endpoints.Development;

internal static class TestsEndpoints
{
    internal static void AddTestsEndpoints(this WebApplication app)
    {
        var testEndpoints = app.MapGroup("/tests")
            .WithTags("Tests");
        
        testEndpoints.MapGet("/isauthenticated", () => "You are authenticated!")
            .RequireAuthorization()
            .WithName("IsAuthenticated");
        
        testEndpoints.MapGet("/ping", () => "pong")
            .WithName("Ping");       
    }
}