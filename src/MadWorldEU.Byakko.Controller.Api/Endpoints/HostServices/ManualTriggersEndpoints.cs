using MadWorldEU.Byakko.Configurations;

namespace MadWorldEU.Byakko.Endpoints.HostServices;

internal static class ManualTriggersEndpoints
{
    internal static void AddManualTriggersEndpoints(this WebApplication app)
    {
        var manualTriggersEndpoint = app.MapGroup("/host-services/manual-triggers")
            .WithTags("Assets")
            .RequireAuthorization(AuthorizationPolicies.Administrator);

        manualTriggersEndpoint.MapPost("/clean-up/assets-content", async (DeleteAllExpiredContentOfAssetsUseCase useCase) =>
        {
            var result = await useCase.ExecuteAsync();
            return result.Match(
                onSuccess: () => Results.Ok(),
                onFailure: error => error.ToBadRequest()
            );
        });

        manualTriggersEndpoint.MapPost("/clean-up/assets-metadata", async (DeleteAllExpiredMetaDataAssetsUseCase useCase) =>
        {
            var result = await useCase.ExecuteAsync();
            return result.Match(
                onSuccess: () => Results.Ok(),
                onFailure: error => error.ToBadRequest()
            );
        });
    }
}