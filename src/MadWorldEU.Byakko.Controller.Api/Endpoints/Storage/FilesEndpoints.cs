namespace MadWorldEU.Byakko.Endpoints.Storage;

public static class FilesEndpoints
{
    public static void AddFilesEndpoints(this WebApplication app)
    {
        var filesEndpoints = app.MapGroup("/files")
            .WithTags("Files");
        
        filesEndpoints.MapPost("/", () =>
            {
                return Results.Ok(Guid.NewGuid().ToString());
            })
            .WithName("CreateFileMetadata");

        filesEndpoints.MapPut("/{id}/content", async (string id, IFormFile file) =>
            {
                await using var stream = file.OpenReadStream();
                return Results.Ok();
            })
            .WithName("UploadFileContent")
            .DisableAntiforgery();
    }
}