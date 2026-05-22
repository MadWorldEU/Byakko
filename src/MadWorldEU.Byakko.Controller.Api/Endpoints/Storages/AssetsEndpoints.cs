using MadWorldEU.Byakko.Configurations;

namespace MadWorldEU.Byakko.Endpoints.Storages;

internal static class AssetsEndpoints
{
    internal static void AddAssetsEndpoints(this WebApplication app)
    {
        var assetsEndpoints = app.MapGroup("/assets")
            .WithTags("Assets");

        assetsEndpoints.MapPost("/", async (CreateAssetRequest request, ClaimsPrincipal user, CreateAssetMetadataUseCase useCase) =>
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst("sub")?.Value
                    ?? string.Empty;

                var result = await useCase.ExecuteAsync(request, userId);
                return result.Match(
                    onSuccess: response => Results.Created($"/assets/{response.Id}", response),
                    onFailure: error => Results.BadRequest(error.Description)
                );
            })
            .RequireAuthorization()
            .WithName("CreateAssetMetadata");

        assetsEndpoints.MapGet("/{id}", async (string id, GetAssetMetadataUseCase useCase) =>
            {
                var result = await useCase.ExecuteAsync(id);
                return result.Match(
                    onSuccess: Results.Ok,
                    onFailure: error => Results.BadRequest(error.Description)
                );
            })
            .WithName("GetAssetMetadata");

        assetsEndpoints.MapPut("/{id}/content", async (string id, IFormFile file, ClaimsPrincipal user, UploadAssetContentUseCase useCase) =>
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst("sub")?.Value
                    ?? string.Empty;

                await using var content = file.OpenReadStream();
                var result = await useCase.ExecuteAsync(id, content, file.Length, userId, file.FileName, file.ContentType);
                return result.Match(
                    onSuccess: Results.Ok,
                    onFailure: error => error.Code == AssetErrors.Forbidden.Code
                        ? Results.Forbid()
                        : Results.BadRequest(error.Description));
            })
            .WithName("UploadAssetContent")
            .DisableAntiforgery()
            .RequireAuthorization()
            .RequireRateLimiting(RateLimiterPolicies.Content);

        assetsEndpoints.MapGet("/{id}/content", async (string id, DownloadAssetContentUseCase useCase) =>
            {
                var result = await useCase.ExecuteAsync(id);
                return result.Match(
                    onSuccess: response => Results.File(response.Content, response.ContentType, response.FileName),
                    onFailure: error => Results.BadRequest(error.Description)
                );
            })
            .WithName("DownloadAssetContent")
            .RequireRateLimiting(RateLimiterPolicies.Content);
    }
}