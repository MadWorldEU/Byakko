using MadWorldEU.Byakko.Storages;

namespace MadWorldEU.Byakko.Endpoints.Storage;

public static class FilesEndpoints
{
    public static void AddFilesEndpoints(this WebApplication app)
    {
        var filesEndpoints = app.MapGroup("/files")
            .WithTags("Files");
        
        filesEndpoints.MapPost("/", (CreateFileMetadataRequest request) =>
            {
                return new CreateFileMetadataResponse()
                {
                    Id = Guid.NewGuid()
                };
            })
            .WithName("CreateFileMetadata");

        filesEndpoints.MapPut("/{id}/content", async (string id, IFormFile file) =>
            {
                await using var stream = file.OpenReadStream();
                return new UploadFileContentResponse();
            })
            .WithName("UploadFileContent")
            .DisableAntiforgery();
    }
}