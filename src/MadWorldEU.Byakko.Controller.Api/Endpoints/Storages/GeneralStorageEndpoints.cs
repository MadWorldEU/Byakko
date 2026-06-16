using MadWorldEU.Byakko.Configurations;

namespace MadWorldEU.Byakko.Endpoints.Storages;

internal static class GeneralStorageEndpoints
{
    internal static void AddGeneralStorageEndpoints(this WebApplication app)
    {
        var storageEndpoints = app.MapGroup("/storage")
            .WithTags("Storage");

        storageEndpoints.MapGet("/statistics", async (GetStorageStatisticsUseCase useCase) =>
            {
                var result = await useCase.QueryAsync();
                return result.Match(
                    onSuccess: Results.Ok,
                    onFailure: error => Results.BadRequest(error.Description)
                );
            })
            .RequireAuthorization(AuthorizationPolicies.Administrator)
            .WithName("GetStorageStatistics");
    }
}