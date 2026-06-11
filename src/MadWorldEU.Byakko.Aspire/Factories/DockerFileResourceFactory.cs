namespace MadWorldEU.Byakko.Factories;

internal sealed class DockerFileResourceFactory(IDistributedApplicationBuilder builder) : IResourceFactory
{
    private const int ApiPort = 5062;
    private const int AdminPort = 5042;
    private const int PortalPort = 5100;
    private const int StatusPort = 5063;
    
    public IResourceBuilder<IResource> CreateApiBuilder(IResourceBuilder<PostgresDatabaseResource> byakkoDb, IResourceBuilder<ILocalStackResource> localstack, IResourceBuilder<KeycloakResource> keycloak)
    {
        return builder.AddDockerfile(nameof(Api), "../../", "src/MadWorldEU.Byakko.Controller.Api/Dockerfile")
            .WithHttpEndpoint(targetPort: 8080, port: ApiPort)
            .BuildApi(byakkoDb, localstack, keycloak);
    }

    public IResourceBuilder<IResource> CreateAdminBuilder(IResourceBuilder<IResource> api)
    {
        return builder.AddDockerfile(nameof(Admin), "../../", "src/MadWorldEU.Byakko.Controller.Admin/Dockerfile")
            .WithHttpEndpoint(targetPort: 8080, port: AdminPort)
            .BuildAdmin(api);
    }

    public IResourceBuilder<IResource> CreatePortalBuilder(IResourceBuilder<IResource> api)
    {
        return builder.AddDockerfile(nameof(Portal), "../../", "src/MadWorldEU.Byakko.Controller.Portal/Dockerfile")
            .WithHttpEndpoint(targetPort: 8080, port: PortalPort)
            .BuildPortal(api);
    }

    public IResourceBuilder<IResource> CreateStatusBuilder(IResourceBuilder<IResource> api, IResourceBuilder<PostgresDatabaseResource> byakkoDb, IResourceBuilder<ILocalStackResource> localstack,
        IResourceBuilder<KeycloakResource> keycloak)
    {
        return builder.AddDockerfile(nameof(StatusPort), "../../", "src/MadWorldEU.Byakko.Controller.Status/Dockerfile")
            .WithHttpEndpoint(targetPort: 8080, port: StatusPort)
            .BuildStatus(api, byakkoDb, localstack, keycloak);
    }
}