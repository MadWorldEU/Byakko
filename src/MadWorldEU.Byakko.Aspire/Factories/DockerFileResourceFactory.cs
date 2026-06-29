namespace MadWorldEU.Byakko.Factories;

internal sealed class DockerFileResourceFactory(IDistributedApplicationBuilder builder) : IResourceFactory
{
    private const string RootFolderSourceCode = "../../";
    
    private const int ApiPort = 5062;
    private const int AdminPort = 5042;
    private const int PortalPort = 5100;
    private const int StatusPort = 5063;
    
    public IResourceBuilder<IResource> CreateApiBuilder(
        IResourceBuilder<PostgresDatabaseResource> byakkoDb, 
        IResourceBuilder<ILocalStackResource> localstack, 
        IResourceBuilder<KeycloakResource> keycloak,
        IResourceBuilder<MailPitContainerResource> mailPit)
    {
        return builder.AddDockerfile(nameof(Api), RootFolderSourceCode, "src/MadWorldEU.Byakko.Controller.Api/Dockerfile")
            .WithHttpEndpoint(targetPort: 8080, port: ApiPort)
            .BuildApi(byakkoDb, localstack, keycloak, mailPit);
    }

    public IResourceBuilder<IResource> CreateAdminBuilder(IResourceBuilder<IResource> api)
    {
        return builder.AddDockerfile(nameof(Admin), RootFolderSourceCode,
                "src/MadWorldEU.Byakko.Controller.Admin/Dockerfile")
            .WithHttpEndpoint(targetPort: 8080, port: AdminPort)
            .BuildAdmin(api);
    }

    public IResourceBuilder<IResource> CreatePortalBuilder(IResourceBuilder<IResource> api)
    {
        return builder.AddDockerfile(nameof(Portal), RootFolderSourceCode,
                "src/MadWorldEU.Byakko.Controller.Portal/Dockerfile")
            .WithHttpEndpoint(targetPort: 8080, port: PortalPort)
            .BuildPortal(api)
            .BuildDockerPortal(PortalPort);
    }

    public IResourceBuilder<IResource> CreateStatusBuilder(IResourceBuilder<PostgresDatabaseResource> byakkoDb, 
        IResourceBuilder<ILocalStackResource> localstack,
        IResourceBuilder<KeycloakResource> keycloak,
        IResourceBuilder<MailPitContainerResource> mailPit)
    {
        return builder.AddDockerfile(nameof(Status), RootFolderSourceCode, "src/MadWorldEU.Byakko.Controller.Status/Dockerfile")
            .WithHttpEndpoint(targetPort: 8080, port: StatusPort)
            .BuildStatus(byakkoDb, localstack, keycloak, mailPit);
    }
}