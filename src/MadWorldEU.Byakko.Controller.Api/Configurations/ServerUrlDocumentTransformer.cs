using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MadWorldEU.Byakko.Configurations;

/// <summary>Overrides the OpenAPI server URL with the configured public base URL so Scalar sends requests to the correct host.</summary>
internal sealed class ServerUrlDocumentTransformer : IOpenApiDocumentTransformer
{
    private readonly IConfiguration _configuration;

    /// <summary>Initialises the transformer with the application configuration.</summary>
    public ServerUrlDocumentTransformer(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>Replaces the auto-detected server list with the configured public URL when present.</summary>
    public Task TransformAsync(OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var publicUrl = _configuration.GetValue<string>("OpenApi:ServerUrl");

        if (!string.IsNullOrWhiteSpace(publicUrl))
        {
            document.Servers = [new OpenApiServer { Url = publicUrl }];
        }

        return Task.CompletedTask;
    }
}