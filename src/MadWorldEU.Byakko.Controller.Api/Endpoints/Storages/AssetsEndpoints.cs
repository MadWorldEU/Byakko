using MadWorldEU.Byakko.Configurations;
using MadWorldEU.Byakko.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace MadWorldEU.Byakko.Endpoints.Storages;

internal static class AssetsEndpoints
{
    internal static void AddAssetsEndpoints(this WebApplication app)
    {
        var assetSettings = app.Services.GetRequiredService<IOptions<AssetSettings>>().Value;
        var maxUploadSizeInBytes = assetSettings.MaxUploadSizeInBytes;

        var assetsEndpoints = app.MapGroup("/assets")
            .WithTags("Assets");

        assetsEndpoints.MapGet("/limits", async (ClaimsPrincipal user, GetUserUploadLimitsUseCase useCase) =>
            {
                var userId = user.GetUserId();

                var result = await useCase.QueryAsync(userId);
                return result.Match(
                    onSuccess: Results.Ok,
                    onFailure: error => error.ToBadRequest()
                );
            })
            .RequireAuthorization()
            .WithName("GetUserUploadLimits");

        assetsEndpoints.MapGet("/", async (int page, Guid? assetId, Guid? userId, GetAssetsMetaDataUseCase useCase) =>
            {
                var result = await useCase.QueryAsync(page, assetId, userId);
                return result.Match(
                    onSuccess: Results.Ok,
                    onFailure: error => error.ToBadRequest()
                );
            })
            .RequireAuthorization(AuthorizationPolicies.Administrator)
            .WithName("GetAllAssetsMetadata");
        
        assetsEndpoints.MapGet("/me", async (int page, Guid? assetId, ClaimsPrincipal user, GetAssetsMetaDataUseCase useCase) =>
            {
                var userId = user.GetUserIdAsGuid();
                var result = await useCase.QueryAsync(page, assetId, userId);
                return result.Match(
                    onSuccess: Results.Ok,
                    onFailure: error => error.ToBadRequest()
                );
            })
            .RequireAuthorization(AuthorizationPolicies.User)
            .WithName("GetMyAssetsMetadata");

        assetsEndpoints.MapPost("/", async (CreateAssetRequest request, ClaimsPrincipal user, HttpContext httpContext, CreateAssetMetadataUseCase useCase) =>
            {
                var userId = user.GetUserId();
                var ipAddress = httpContext.Connection.RemoteIpAddress;

                var result = await useCase.ExecuteAsync(request, userId, ipAddress);
                return result.Match(
                    onSuccess: response => Results.Created($"/assets/{response.Id}", response),
                    onFailure: error => error.ToBadRequest()
                );
            })
            .RequireAuthorization()
            .WithName("CreateAssetMetadata");

        assetsEndpoints.MapGet("/{id}", async (string id, GetAssetMetadataUseCase useCase) =>
            {
                var result = await useCase.QueryAsync(id);
                return result.Match(
                    onSuccess: Results.Ok,
                    onFailure: error => error.Code == AssetErrors.NotFound.Code
                        ? error.ToNotFound()
                        : error.ToBadRequest()
                );
            })
            .WithName("GetAssetMetadata");

        assetsEndpoints.MapPut("/{id}/content",
                async (string id, IFormFile file, ClaimsPrincipal user, HttpContext httpContext, UploadAssetContentUseCase useCase) =>
                {
                    var userId = user.GetUserId();
                    var ipAddress = httpContext.Connection.RemoteIpAddress;

                    await using var content = file.OpenReadStream();
                    var result = await useCase.ExecuteAsync(id, content, file.Length, userId, ipAddress, file.FileName,
                        file.ContentType);
                    return result.Match(
                        onSuccess: Results.Ok,
                        onFailure: error => error.Code == AssetErrors.NotFound.Code
                            ? error.ToNotFound()
                            : error.Code == AssetErrors.Forbidden.Code
                                ? Results.Forbid()
                                : error.ToBadRequest());
                })
            .WithName("UploadAssetContent")
            .WithMetadata(new RequestSizeLimitAttribute(maxUploadSizeInBytes))
            .WithMetadata(new RequestFormLimitsAttribute { MultipartBodyLengthLimit = maxUploadSizeInBytes })
            .DisableAntiforgery()
            .RequireAuthorization()
            .RequireRateLimiting(RateLimiterPolicies.Content);

        assetsEndpoints.MapDelete("/{id}/content", async (string id, HttpContext httpContext, DeleteContentOfAssetUseCase useCase) =>
            {
                var ipAddress = httpContext.Connection.RemoteIpAddress;

                var result = await useCase.ExecuteAsync(id, ipAddress);
                return result.Match(
                    onSuccess: Results.Ok,
                    onFailure: error => error.Code == AssetErrors.NotFound.Code
                        ? error.ToNotFound()
                        : error.Code == AssetErrors.AlreadyDeleted.Code
                            ? error.ToConflict()
                            : error.ToBadRequest()
                );
            })
            .RequireAuthorization(AuthorizationPolicies.Administrator)
            .WithName("DeleteAssetContent");

        assetsEndpoints.MapDelete("/me/{id}/content", async (string id, ClaimsPrincipal user, HttpContext httpContext, DeleteMyAssetContentUseCase useCase) =>
            {
                var userId = user.GetUserId();
                var ipAddress = httpContext.Connection.RemoteIpAddress;

                var result = await useCase.ExecuteAsync(id, userId, ipAddress);
                return result.Match(
                    onSuccess: Results.Ok,
                    onFailure: error => error.Code == AssetErrors.NotFound.Code
                        ? error.ToNotFound()
                        : error.Code == AssetErrors.Forbidden.Code
                            ? Results.Forbid()
                            : error.Code == AssetErrors.AlreadyDeleted.Code
                                ? error.ToConflict()
                                : error.ToBadRequest()
                );
            })
            .RequireAuthorization(AuthorizationPolicies.User)
            .WithName("DeleteMyAssetContent");

        assetsEndpoints.MapGet("/{id}/content", async (string id, DownloadAssetContentUseCase useCase) =>
            {
                var result = await useCase.QueryAsync(id);
                
                return result.Match(
                    onSuccess: response => Results.File(response.Content, response.ContentType, response.FileName),
                    onFailure: error => error.Code == AssetErrors.NotFound.Code
                        ? error.ToNotFound()
                        : error.ToBadRequest()
                );
            })
            .WithName("DownloadAssetContent")
            .RequireRateLimiting(RateLimiterPolicies.Content);
    }
}