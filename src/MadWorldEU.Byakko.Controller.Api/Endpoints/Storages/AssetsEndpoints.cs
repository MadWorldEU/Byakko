namespace MadWorldEU.Byakko.Endpoints.Storages;

public static class AssetsEndpoints
{
    public static void AddAssetsEndpoints(this WebApplication app)
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

        assetsEndpoints.MapPut("/{id}/content", async (string id, IFormFile file, UploadAssetContentUseCase useCase) =>
            {
                await using var stream = file.OpenReadStream();
                useCase.ExecuteAsync();
                return new UploadAssetContentResponse();
            })
            .WithName("UploadAssetContent")
            .DisableAntiforgery();
    }
}