using MadWorldEU.Byakko.Storages;

namespace MadWorldEU.Byakko.Endpoints.Storages;

public static class AssetsEndpoints
{
    public static void AddAssetsEndpoints(this WebApplication app)
    {
        var assetsEndpoints = app.MapGroup("/assets")
            .WithTags("Assets");

        assetsEndpoints.MapPost("/", (CreateFileMetadataRequest request) =>
            {
                return new CreateFileMetadataResponse()
                {
                    Id = Guid.NewGuid()
                };
            })
            .WithName("CreateAssetMetadata");

        assetsEndpoints.MapPut("/{id}/content", async (string id, IFormFile file) =>
            {
                await using var stream = file.OpenReadStream();
                return new UploadFileContentResponse();
            })
            .WithName("UploadAssetContent")
            .DisableAntiforgery();
    }
}