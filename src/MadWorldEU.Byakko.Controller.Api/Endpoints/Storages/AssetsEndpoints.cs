namespace MadWorldEU.Byakko.Endpoints.Storages;

internal static class AssetsEndpoints
{
    internal static void AddAssetsEndpoints(this WebApplication app)
    {
        var assetsEndpoints = app.MapGroup("/assets")
            .WithTags("Assets");

        assetsEndpoints.MapPost("/", async (CreateAssetRequest request, CreateAssetMetadataUseCase useCase) =>
            {
                var result = await useCase.ExecuteAsync(request);
                return result.Match(
                    onSuccess: response => Results.Created($"/assets/{response.Id}", response),
                    onFailure: error => Results.BadRequest(error.Description)
                );
            })
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

        assetsEndpoints.MapPut("/{id}/content", async (string id, IFormFile file, UploadAssetContentUseCase useCase) =>
            {
                await using var content = file.OpenReadStream();
                var result = await useCase.ExecuteAsync(id, content);
                return result.Match(
                    onSuccess: Results.Ok,
                    onFailure: error => Results.BadRequest(error.Description));
            })
            .WithName("UploadAssetContent")
            .DisableAntiforgery();
        
        assetsEndpoints.MapGet("/{id}/content", async (string id, DownloadAssetContentUseCase useCase) =>
            {
                var result = await useCase.ExecuteAsync(id);
                return result.Match(
                    onSuccess: response => Results.File(response.Content, response.ContentType, response.FileName),
                    onFailure: error => Results.BadRequest(error.Description)
                );
            })
            .WithName("DownloadAssetContent");
    }
}